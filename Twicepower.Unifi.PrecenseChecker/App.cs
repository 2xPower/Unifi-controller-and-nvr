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
            sb.AppendLine("Without a command specification the camera's wil be updated according to your configuration.");
            sb.AppendLine("If you have issues of feature requests please go to https://github.com/2xPower/Unifi-controller-and-nvr");
            commandLineApplication.ExtendedHelpText = sb.ToString();


            #region Config

            var nvrConfig = _config.GetSection("video").Get<NvrConfig>();
            var controllerConfig = _config.GetSection("controller").Get<ControllerConfig>();
            var presenceConfig = _config.GetSection("presence").Get<PresenceRecordingSettings>();

             // configuration check
            _logger.LogDebug($"{nameof(controllerConfig)} is null: {controllerConfig == null}");
            _logger.LogDebug($"{nameof(controllerConfig)} base url: {controllerConfig?.BaseUrl}");

            _logger.LogDebug($"{nameof(nvrConfig)} is null: {nvrConfig == null}");
            _logger.LogDebug($"{nameof(nvrConfig)} base url: {nvrConfig?.BaseUrl}");
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


            commandLineApplication.Command("list-clients",
              (target) =>
              {
                  target.OnExecute(async () =>
                  {
                      await OutputConnectedClientsToConsole(controllerConfig, presenceConfig);
                  });
              });

            commandLineApplication.Command("list-cameras",
              (target) =>
              {
                  target.OnExecute(async () =>
                  {
                      await OutputConnectedCamerasToConsole(nvrConfig, presenceConfig);
                  });
              });

            commandLineApplication.Command("config",
                  (target) =>
                  {
                      target.OnExecute(async () =>
                      {
                          Console.WriteLine("Configuration".ToUpper());
                          Console.WriteLine("-------------");

                          Console.WriteLine("[General]");
                          if(Prompt.GetYesNo("Use SOCKS proxy for connection to Unifi: ", false))
                          {
                              presenceConfig.SOCKS = Prompt.GetString("Input the url for proxy (eg. http://localhost:8888");
                          }
                          presenceConfig.VerifySsl = Prompt.GetYesNo("Verify SSL certificate when connecting to unifi: ", true);
                          Console.WriteLine();
                          Console.WriteLine("[Unifi Controller settings]");
                          optionBaseUrlController.Values.Insert(0, Prompt.GetString("Unifi controller URL: "));
                          optionSitename.Values.Insert(0, Prompt.GetString("Sitename (if other then default): ", "default"));
                          optionUserNameController.Values.Insert(0, Prompt.GetString("Username: "));
                          optionPassWordController.Values.Insert(0, Prompt.GetPassword("Password: "));
                          MergeConfig(controllerConfig, optionSitename, optionUserNameController, optionPassWordController, optionBaseUrlController);
                          Console.WriteLine("Testing configuration for Unifi Controller");
                          var connectedClients = await GetConnectedClients(controllerConfig, presenceConfig);
                          WriteConnectedClientsToConsole(connectedClients);

                          var clientIndexes = Prompt.GetString("Comma seperated list of client Index number (eg: 2,15,7): ").Split(",", StringSplitOptions.RemoveEmptyEntries);
                          var clientList = new List<Sta>();
                          foreach (var index in clientIndexes)
                          {
                              clientList.Add(connectedClients[int.Parse(index)]);
                          }
                          presenceConfig.PresenceIndicationMACs = clientList.Select(s => s.Mac).ToArray();

                          Console.WriteLine();
                          Console.WriteLine("[Unifi Controller settings]");
                          baseUrlNvr.Values.Insert(0, Prompt.GetString("Unifi Video URL: "));
                          userNameNvr.Values.Insert(0, Prompt.GetString("Username: "));
                          passWordNvr.Values.Insert(0, Prompt.GetPassword("Password: "));
                          MergeConfig(controllerConfig, optionSitename, optionUserNameController, optionPassWordController, optionBaseUrlController);
                          Console.WriteLine("Testing configuration for Unifi Video");
                          MergeConfig(nvrConfig, userNameNvr, passWordNvr, baseUrlNvr);
                          var status = await GetNvrStatus(nvrConfig, presenceConfig);
                          WriteNvrStatusToConsole(status);

                          clientIndexes = Prompt.GetString("Comma seperated list of camera index number (eg: 2,15,7) for wich recording mode should be managed: ").Split(",", StringSplitOptions.RemoveEmptyEntries);
                          var cameraList = new List<Camera>();
                          foreach (var index in clientIndexes)
                          {
                              cameraList.Add(status.Cameras[int.Parse(index)]);
                          }

                          presenceConfig.CameraIdsToSetToMotionRecordingIfNoOneIsPresent = cameraList.Select(s => s.Id).ToArray();

                          var configString = Newtonsoft.Json.JsonConvert.SerializeObject(new { video = nvrConfig, controller = controllerConfig, presence = presenceConfig });

                          Console.WriteLine();
                          Console.WriteLine("New configuration:");
                          Console.WriteLine();
                          Console.WriteLine(configString);

                          if (Prompt.GetYesNo("Write this configuration to appsettings.json?: ", true))
                          {
                              System.IO.File.WriteAllBytes(System.IO.Path.Combine(Program.InstalledPath, Program.configFileName), Encoding.UTF8.GetBytes(configString));
                          }

                      });
                  });

            commandLineApplication.Command("update", (target) => 
            {
                target.OnExecute(async () => 
                {
                    var currentTime = DateTime.Now.TimeOfDay;
                    bool shouldRecord = (currentTime.Hours >= 23 || currentTime.Hours < 8) || !IsOneOrMoreMACPresent(await GetConnectedClients(controllerConfig, presenceConfig), presenceConfig);

                    _logger.LogInformation($"Should record: {shouldRecord}");
                    var nvrClient = new TwicePower.Unifi.UnifiVideoClient(GetHttpClient(nvrConfig.BaseUrl, presenceConfig.SOCKS, presenceConfig.VerifySsl));
                    var status = await GetNvrStatus(nvrClient, nvrConfig);
                    if (status != null)
                    {
                        UpdateCameraRecordingState(nvrClient, status, presenceConfig, shouldRecord).GetAwaiter().GetResult();
                    }
                    return 0;
                });
            });

            commandLineApplication.OnExecute(() =>
            {
                commandLineApplication.ShowHelp();
                return 0;
            });

            commandLineApplication.Parse(args);

            MergeConfig(controllerConfig, optionSitename, optionUserNameController, optionPassWordController, optionBaseUrlController);
            MergeConfig(nvrConfig, userNameNvr, passWordNvr, baseUrlNvr);


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

        private async Task OutputConnectedCamerasToConsole(NvrConfig nvrConfig, PresenceRecordingSettings presenceConfig)
        {
            Status status = await GetNvrStatus(nvrConfig, presenceConfig);
            if (status != null)
            {
                WriteNvrStatusToConsole(status);
            }
        }

        private async Task<Status> GetNvrStatus(NvrConfig nvrConfig, PresenceRecordingSettings presenceConfig)
        {
            var nvrClient = new TwicePower.Unifi.UnifiVideoClient(GetHttpClient(nvrConfig.BaseUrl, presenceConfig.SOCKS, presenceConfig.VerifySsl));
            var status = await GetNvrStatus(nvrClient, nvrConfig);
            return status;
        }

        private void WriteNvrStatusToConsole(Status status)
        {
            string tableHeader = $"Index {"Name and OSD tag".PadRight(30, ' ')} {"ID".PadRight(30, ' ')}{"State".PadRight(20, ' ')} Record on motion";
            Console.WriteLine();
            Console.WriteLine(tableHeader);
            Console.WriteLine("".PadRight(tableHeader.Length, '_'));
            for (int i = 0; i < status.Cameras.Length; i++)
            {
                var camera = status.Cameras[i];
                var description = GetCameraDescription(camera);
                Console.WriteLine($"{i.ToString().PadRight(6)}{description.PadRight(30, ' ')} {camera.Id.PadRight(30, ' ')}{camera.State.PadRight(20, ' ')} {camera.RecordingSettings.MotionRecordEnabled}");
            }
        }

        private async Task OutputConnectedClientsToConsole(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig)
        {
            var connectedClients = await GetConnectedClients(controllerConfig, presenceConfig);
            if (connectedClients?.Any() == true)
            {
                WriteConnectedClientsToConsole(connectedClients);
            }
        }

        private static void WriteConnectedClientsToConsole(Sta[] connectedClients)
        {
            string tableHeader = $"Index {"MAC".PadRight(20, ' ')} {"Host name".PadRight(30, ' ')}Idle time";
            Console.WriteLine();
            Console.WriteLine(tableHeader);
            Console.WriteLine("".PadRight(tableHeader.Length, '_'));
            var wireLessClients = connectedClients.Where(client => client.IsWired == false).ToArray();
            for (int i = 0; i < wireLessClients.Length; i++)
            {
                var client = wireLessClients[i];
                Console.WriteLine($"{i.ToString().PadRight(6)}{client.Mac.PadRight(20, ' ')} {client.Hostname ?? "".PadRight(30, ' ')}{client.Idletime}");
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

        private async Task<Sta[]> GetConnectedClients(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig)
        {
            var controllerClient = new UnifiControllerClient(GetHttpClient(controllerConfig.BaseUrl, presenceConfig.SOCKS, presenceConfig.VerifySsl));

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
