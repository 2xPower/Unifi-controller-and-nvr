using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using McMaster.Extensions.CommandLineUtils;
using TwicePower.Unifi.Controller;
using TwicePower.Unifi.Nvr;

namespace TwicePower.Unifi.PrecenseChecker
{
    public class App
    {
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _config;

        public App(ILogger logger, IConfigurationRoot config)
        {
            this._logger = logger;
            this._config = config;
        }

        public async Task<int> Run(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            CommandOption optionUserNameController = commandLineApplication.Option("-uc |--username-controller <username>",
                $"The user name  for unifi controller.", CommandOptionType.SingleValue);
            CommandOption optionPassWordController = commandLineApplication.Option("-pc |--password-controller <password>",
               $"The password for unifi controller.", CommandOptionType.SingleValue);
            CommandOption optionBaseUrlController = commandLineApplication.Option("-urlc | --url-controller <url>",
               $"The url for the Unifi Controller.", CommandOptionType.SingleValue);
            CommandOption optionSitename = commandLineApplication.Option("-s | --site-description <sitedescription>",
               $"The description of site, in the controller, to connect to.", CommandOptionType.SingleValue);

            CommandOption userNameNvr = commandLineApplication.Option("-uv | --username-video <username>",
               "The user name  for Unifi Video.", CommandOptionType.SingleValue);
            CommandOption passWordNvr = commandLineApplication.Option("-pv | --password-video <password>",
               "The password for Unifi Video.", CommandOptionType.SingleValue);
            CommandOption baseUrlNvr = commandLineApplication.Option("-urlv | --url-video <url>",
               "The url for the Unifi Video .", CommandOptionType.SingleValue);
            commandLineApplication.HelpOption("-? | -h | --help");
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Without a command specification the camera's wil be updated accoring to your configuration.");
            sb.AppendLine("If you have issues of feature requests please go to https://github.com/2xPower/Unifi-controller-and-nvr");
            commandLineApplication.ExtendedHelpText = sb.ToString();


            #region Config

            var nvrConfig = _config.GetSection("nvr").Get<NvrConfig>() ?? new NvrConfig();
            var controllerConfig = _config.GetSection("controller").Get<ControllerConfig>();
            var presenceConfig = _config.GetSection("presence").Get<PresenceRecordingSettings>();

            // configuration check
            _logger.LogDebug($"{nameof(controllerConfig)} is null: {controllerConfig == null}");
            _logger.LogDebug($"{nameof(controllerConfig)} base url: {controllerConfig.BaseUrl}");

            _logger.LogDebug($"{nameof(nvrConfig)} is null: {nvrConfig == null}");
            _logger.LogDebug($"{nameof(nvrConfig)} base url: {nvrConfig.BaseUrl}");
            _logger.LogDebug($"{nameof(presenceConfig)} is null: {presenceConfig == null}");

            if(controllerConfig == null)
            {
                _logger.LogWarning("Failed to load Unifi Controller configuration information from config file.");
                controllerConfig = new ControllerConfig();
            }
            if (nvrConfig == null)
            {
                _logger.LogWarning("Failed to load Unifi video (nvr) configuration information from config file.");
                nvrConfig = new NvrConfig();
            }
            if(presenceConfig == null)
            {
                _logger.LogWarning("Failed to load presence configuration information from config file.");
                presenceConfig = new PresenceRecordingSettings();
            }
            #endregion

            MergeConfig(controllerConfig, optionSitename, optionUserNameController, optionPassWordController, optionBaseUrlController);
            MergeConfig(nvrConfig, userNameNvr, passWordNvr, baseUrlNvr);


            commandLineApplication.Command("list-clients",
              (target) =>
              {
                  target.OnExecute(async () =>
                  {
                      await OutputConnectedClientsToConsole(controllerConfig);
                  });
              });

            commandLineApplication.Command("list-cameras",
              (target) =>
              {
                  target.OnExecute(async () =>
                  {
                      await OutputConnectedCamerasToConsole(nvrConfig);
                  });
              });

            commandLineApplication.OnExecute(async () =>
            {
                var currentTime = DateTime.Now.TimeOfDay;
                bool shouldRecord = (currentTime.Hours >= 23 || currentTime.Hours < 8) || !IsOneOrMoreMACPresent(await GetConnectedClients(controllerConfig), presenceConfig);

                _logger.LogInformation($"Should record: {shouldRecord}");
                var nvrClient = new TwicePower.Unifi.UnifiVideoClient(GetHttpClient(nvrConfig.BaseUrl, nvrConfig.SocksProxy, nvrConfig.VerifySsl));
                var status = await GetNvrStatus(nvrClient, nvrConfig);
                if (status != null)
                {
                    UpdateCameraRecordingState(nvrClient, status, presenceConfig, shouldRecord).GetAwaiter().GetResult();
                }


                return 0;
            });
            commandLineApplication.Execute(args);

            return 0;
        }

        private void MergeConfig(NvrConfig nvrConfig, CommandOption userNameNvr, CommandOption passWordNvr, CommandOption baseUrlNvr)
        {
            if (userNameNvr?.HasValue() == true)
            {
                nvrConfig.UserName = userNameNvr.Value();
            }
            if (passWordNvr?.HasValue() == true)
            {
                nvrConfig.Password = passWordNvr.Value();
            }
            if (baseUrlNvr?.HasValue() == true)
            {
                nvrConfig.BaseUrl = baseUrlNvr.Value();
            }
        }

        private void MergeConfig(ControllerConfig controllerConfig, CommandOption optionSitename, CommandOption optionUserNameController, CommandOption optionPassWordController, CommandOption optionBaseUrlController)
        {
            if(optionSitename?.HasValue() == true)
            {
                controllerConfig.ControllerSiteDescription = optionSitename.Value();
            }
            if (optionUserNameController?.HasValue() == true)
            {
                controllerConfig.UserName = optionUserNameController.Value();
            }
            if (optionPassWordController?.HasValue() == true)
            {
                controllerConfig.Password = optionPassWordController.Value();
            }
            if (optionBaseUrlController?.HasValue() == true)
            {
                controllerConfig.BaseUrl = optionBaseUrlController.Value();
            }
        }

        private async Task OutputConnectedCamerasToConsole(NvrConfig nvrConfig)
        {
            var nvrClient = new TwicePower.Unifi.UnifiVideoClient(GetHttpClient(nvrConfig.BaseUrl, nvrConfig.SocksProxy, nvrConfig.VerifySsl));
            var status = await GetNvrStatus(nvrClient, nvrConfig);
            if (status != null)
            {
                string tableHeader = $"{"Name and OSD tag".PadRight(30, ' ')} {"ID".PadRight(30, ' ')}{"State".PadRight(20, ' ')} Record on motion";
                Console.WriteLine();
                Console.WriteLine(tableHeader);
                Console.WriteLine("".PadRight(tableHeader.Length, '_'));
                foreach (var camera in status.Cameras)
                {
                    var description = GetCameraDescription(camera);
                    Console.WriteLine($"{description.PadRight(30, ' ')} {camera.Id.PadRight(30, ' ')}{camera.State.PadRight(20, ' ')} {camera.RecordingSettings.MotionRecordEnabled}");
                }
            }
        }

        private async Task OutputConnectedClientsToConsole(ControllerConfig controllerConfig)
        {
            var connectedClients = await GetConnectedClients(controllerConfig);
            if (connectedClients?.Any() == true)
            {
                string tableHeader = $"{"MAC".PadRight(20, ' ')} {"Host name".PadRight(30, ' ')}Idle time";
                Console.WriteLine();
                Console.WriteLine(tableHeader);
                Console.WriteLine("".PadRight(tableHeader.Length, '_'));
                foreach (var client in connectedClients.Where(client => client.IsWired == false).ToArray())
                {
                    Console.WriteLine($"{client.Mac.PadRight(20, ' ')} {client.Hostname ?? "".PadRight(30, ' ')}{client.Idletime}");
                }
            }
        }

        private bool IsOneOrMoreMACPresent(IEnumerable<Sta> connectedDevices , PresenceRecordingSettings presenceConfig)
        {
            if (connectedDevices?.Count() > 0)
            {

                var precenseIndicatingDevices = connectedDevices.Where(a => presenceConfig.PresenceIndicationMACs.Any(b => string.Compare(b, a.Mac, StringComparison.InvariantCultureIgnoreCase) == 0)).ToArray();

                if (precenseIndicatingDevices?.Any() == true)
                {
                    foreach (var device in precenseIndicatingDevices)
                    {
                        _logger.LogInformation($"{device.Hostname} is connected.");
                    }
                }
                else
                {
                    _logger.LogInformation("None of the configured MAC are connected.");
                }
                return precenseIndicatingDevices?.Any() == true;
            }
            else
            {
                return false;
            }
        }

        private async Task<Sta[]> GetConnectedClients(ControllerConfig controllerConfig)
        {
            var controllerClient = new UnifiControllerClient(GetHttpClient(controllerConfig.BaseUrl, controllerConfig.SocksProxy, controllerConfig.VerifySsl));

            var loginResult = await controllerClient.Login(controllerConfig.UserName, controllerConfig.Password);
            if (loginResult == true)
            {
                var siteName = controllerClient.GetSites().Result.FirstOrDefault(p => string.Compare(controllerConfig.ControllerSiteDescription, p.Desc, StringComparison.InvariantCultureIgnoreCase) == 0).Name;
                if (siteName == null)
                {
                    _logger.LogError($"The controller site description {controllerConfig.ControllerSiteDescription} could not be found.");
                }
                else
                {
                    var connectedDevices = await controllerClient.GetConnectedClients(siteName);
                    return connectedDevices;
                }
            }
            else { _logger.LogError("Failed to log on to the controller."); }

            return null;
        }

        private async Task<Nvr.Status> GetNvrStatus(UnifiVideoClient nvrClient, NvrConfig nvrConfig)
        {
            if (await nvrClient.Login(nvrConfig.UserName, nvrConfig.Password))
            {
                var status = await nvrClient.GetStatus();
                _logger.LogInformation($"Retrieved status from Unifi Video and found {status?.Cameras?.Count()} camera's.");
                return status;
            }
            else
            {
                _logger.LogError("Failed to log on to the NVR");
            }
            return null;
        }
        private string GetCameraDescription(Camera camera)
        {
            return $"{camera.Name} - {camera.OsdSettings.Tag}";
        }

        private async Task UpdateCameraRecordingState(UnifiVideoClient nvrClient, Nvr.Status status, PresenceRecordingSettings presenceConfig, bool shouldRecord)
        {
            foreach (var cameraId in presenceConfig.CameraIdsToSetToMotionRecordingIfNoOneIsPresent)
            {
                var camera = status.Cameras.FirstOrDefault(p => p.Id == cameraId);
                var cameraDescription = GetCameraDescription(camera);

                _logger.LogInformation($"Camera {cameraDescription} is recording motion: {camera.RecordingSettings.MotionRecordEnabled}");
                if (camera != null && camera.RecordingSettings?.MotionRecordEnabled != shouldRecord)
                {
                    _logger.LogInformation($"Updating camera {cameraDescription}");
                    camera.RecordingSettings.MotionRecordEnabled = camera.EnableStatusLed = camera.LedFaceAlwaysOnWhenManaged = shouldRecord;
                    await nvrClient.UpdateCamera(camera);
                }
                else
                {
                    _logger.LogInformation($"Update for camera ({cameraDescription}) not required.");
                }
            }
        }

        private static HttpClient GetHttpClient(string baseUrl, string socksProxy = null, bool sslVerify = true)
        {
            SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler();
            if (!string.IsNullOrWhiteSpace(socksProxy) && Uri.IsWellFormedUriString(socksProxy, UriKind.Absolute))
            {
                socketsHttpHandler.UseProxy = true;
                socketsHttpHandler.Proxy = new WebProxy(socksProxy, false);
            }

            if (!sslVerify)
            {
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
                socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback += (a, b, c, d) =>
                {
                    return true;
                };
            }
            HttpClient httpClient = new HttpClient(socketsHttpHandler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "deflate");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Twicepower-Unifi-Client");



            return httpClient;
        }
    }
}
