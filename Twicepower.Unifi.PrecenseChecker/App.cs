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
            var helpoptions = "-? | -h | --help";
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
            commandLineApplication.HelpOption(helpoptions);
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("When using any of the command line options for configuration, these values are used instead of the values from appsettings.json");
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


            commandLineApplication.Command("show",
              (target) =>
              {
                  target.Description = "Show different types of information (wireless clients, cameras, ...)";
                  target.HelpOption(helpoptions);
                    target.Command("clients", (listClientCommand) => 
                    {
                        listClientCommand.Description = "Shows the list of connected wireless clients for the configured Unifi Controller and Site";
                        listClientCommand.OnExecute(async () => { await OutputConnectedClientsToConsole(controllerConfig, presenceConfig, _logger); });
                    });
                    target.Command("cameras", (listCameraCommand) => 
                    {
                        listCameraCommand.Description = "Shows the list of camera's managed by the configured Unifi NVR";
                        listCameraCommand.OnExecute(async () => { await OutputConnectedCamerasToConsole(nvrConfig, presenceConfig, _logger); });
                    });

                    target.OnExecute( () => 
                    {
                        Console.WriteLine("Specify a subcommand");
                        target.ShowHelp();
                        return 1;
                    });
              });

            commandLineApplication.Command("config",
                  (target) =>
                  {
                      target.HelpOption(helpoptions);
                      target.Description = "Starts the user prompts to create or update the configuration stored in appsettings.json";
                      target.OnExecute(async () =>
                      {
                          Console.WriteLine();
                          Console.WriteLine("-------------");
                          Console.WriteLine("Configuration".ToUpper());
                          Console.WriteLine("-------------");
                          Console.WriteLine("Press Enter to leave current value unchaged");
                          Console.WriteLine();

                          SetGeneralConfig(presenceConfig);
                          SetControllerConfig(optionUserNameController, optionPassWordController, optionBaseUrlController, optionSitename, controllerConfig);

                          await UpdateConfigDevices(controllerConfig, presenceConfig, _logger);

                          UpdateConfigUnifiVideo(userNameNvr, passWordNvr, baseUrlNvr, nvrConfig);

                          await UpdateConfigCameras(nvrConfig, presenceConfig, _logger);

                          var configString = Newtonsoft.Json.JsonConvert.SerializeObject(new { video = nvrConfig, controller = controllerConfig, presence = presenceConfig });

                          Console.WriteLine();
                          Console.WriteLine("New configuration:");
                          Console.WriteLine();
                          Console.WriteLine(configString);

                          if (Prompt.GetYesNo("Write this configuration to appsettings.json?: ", true))
                          {
                              System.IO.File.WriteAllBytes(System.IO.Path.Combine(Program.InstalledPath, Program.configFileName), Encoding.UTF8.GetBytes(configString));
                              _logger.LogInformation("appsettings.json written");
                          }

                      });
                  });

            commandLineApplication.Command("update", (target) => 
            {
                target.Description = "Update the configured camera's based on presence of the configured MAC addresses on the wifi network.";
                target.OnExecute(async () => 
                {
                    var currentTime = DateTime.Now.TimeOfDay;
                    bool shouldRecord = (presenceConfig.EnableNightRecordingIfAtHome && (currentTime.Hours >= 23 || currentTime.Hours < 8)) || !IsOneOrMoreMACPresent(await GetConnectedClients(controllerConfig, presenceConfig, _logger), presenceConfig);

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

        private static async Task UpdateConfigCameras(NvrConfig nvrConfig, PresenceRecordingSettings presenceConfig, ILogger logger)
        {
            Console.WriteLine("Testing configuration for Unifi Video");
            var status = await GetNvrStatus(nvrConfig, presenceConfig, logger);
            WriteNvrStatusToConsole(status);

            var camIndexes = Prompt.GetString("Comma seperated list of camera index number (eg: 2,15,7) for wich recording mode should be managed: ").Split(",", StringSplitOptions.RemoveEmptyEntries);
            var cameraList = new List<Camera>();
            foreach (var index in camIndexes)
            {
                cameraList.Add(status.Cameras[int.Parse(index)]);
            }

            presenceConfig.CameraIdsToSetToMotionRecordingIfNoOneIsPresent = cameraList.Select(s => s.Id).ToArray();
        }

        private static void UpdateConfigUnifiVideo(CommandOption userNameNvr, CommandOption passWordNvr, CommandOption baseUrlNvr, NvrConfig nvrConfig)
        {
            Console.WriteLine("[Unifi Controller settings]");
            baseUrlNvr.Values.Insert(0, Prompt.GetString("Unifi Video URL: ", nvrConfig.BaseUrl));
            userNameNvr.Values.Insert(0, Prompt.GetString("Username: ", nvrConfig.UserName));
            passWordNvr.Values.Insert(0, Prompt.GetString("Password - saved in plain text in the settings file: ", nvrConfig.Password));
            MergeConfig(nvrConfig, userNameNvr, passWordNvr, baseUrlNvr);
            Console.WriteLine();
        }

        private static async Task UpdateConfigDevices(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig, ILogger logger)
        {
            Console.WriteLine("Testing configuration for Unifi Controller");
            var connectedClients = await GetConnectedClients(controllerConfig, presenceConfig, logger);
            var wireLessClients = connectedClients.Where(client => client.IsWired == false).ToArray();
            WriteConnectedClientsToConsole(wireLessClients);

            var clientIndexes = Prompt.GetString("Comma seperated list of client Index number (eg: 2,15,7): ").Split(",", StringSplitOptions.RemoveEmptyEntries);
            var clientList = new List<Sta>();
            foreach (var index in clientIndexes)
            {
                clientList.Add(wireLessClients[int.Parse(index)]);
            }
            presenceConfig.PresenceIndicationMACs = clientList.Select(s => s.Mac).ToArray();
            Console.WriteLine();
        }

        private static void SetControllerConfig(CommandOption optionUserNameController, CommandOption optionPassWordController, CommandOption optionBaseUrlController, CommandOption optionSitename, ControllerConfig controllerConfig)
        {
            Console.WriteLine("[Unifi Controller settings]");
            optionBaseUrlController.Values.Insert(0, Prompt.GetString("Unifi controller URL: ", controllerConfig.BaseUrl));
            optionSitename.Values.Insert(0, Prompt.GetString("Site name as displayed in the Controller user interface: ", controllerConfig.ControllerSiteDescription));
            optionUserNameController.Values.Insert(0, Prompt.GetString("Username: ", controllerConfig.UserName));
            optionPassWordController.Values.Insert(0, Prompt.GetString("Password - saved in plain text in the settings file: ", controllerConfig.Password));
            MergeConfig(controllerConfig, optionSitename, optionUserNameController, optionPassWordController, optionBaseUrlController);
        }

        private static void SetGeneralConfig(PresenceRecordingSettings presenceConfig)
        {
            Console.WriteLine("[General]");
            if (Prompt.GetYesNo("Use SOCKS proxy for connection to Unifi: ", false))
            {
                presenceConfig.SOCKS = Prompt.GetString("Input the url for proxy (eg. http://localhost:8888");
            }
            presenceConfig.VerifySsl = Prompt.GetYesNo("Verify SSL certificate when connecting to unifi: ", true);
            presenceConfig.EnableNightRecordingIfAtHome = Prompt.GetYesNo("Would you like to record motion at night (23h-8h) even if someone is at home? ", presenceConfig.EnableNightRecordingIfAtHome);
            Console.WriteLine();
        }

        private static void MergeConfig(NvrConfig nvrConfig, CommandOption userNameNvr, CommandOption passWordNvr, CommandOption baseUrlNvr)
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

        private static void MergeConfig(ControllerConfig controllerConfig, CommandOption optionSitename, CommandOption optionUserNameController, CommandOption optionPassWordController, CommandOption optionBaseUrlController)
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

        private static async Task OutputConnectedCamerasToConsole(NvrConfig nvrConfig, PresenceRecordingSettings presenceConfig, ILogger logger)
        {
            Status status = await GetNvrStatus(nvrConfig, presenceConfig, logger);
            if (status != null)
            {
                WriteNvrStatusToConsole(status);
            }
        }

        private static async Task<Status> GetNvrStatus(NvrConfig nvrConfig, PresenceRecordingSettings presenceConfig, ILogger logger)
        {
            var nvrClient = new TwicePower.Unifi.UnifiVideoClient(GetHttpClient(nvrConfig.BaseUrl, presenceConfig.SOCKS, presenceConfig.VerifySsl));
            if(await nvrClient.Login(nvrConfig.UserName, nvrConfig.Password))
            {
                var status = await nvrClient.GetStatus();
                return status;
            }
            else
            {
                logger.LogError("Failed to log on to Unifi Video");
            }
            return null;
        }

        private static void WriteNvrStatusToConsole(Status status)
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

        private async Task OutputConnectedClientsToConsole(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig, ILogger logger)
        {
            var connectedClients = await GetConnectedClients(controllerConfig, presenceConfig, logger);
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
            for (int i = 0; i < connectedClients.Length; i++)
            {
                var client = connectedClients[i];
                Console.WriteLine($"{i.ToString().PadRight(6)}{client.Mac.PadRight(20, ' ')} {(client.Hostname ?? "").PadRight(30, ' ')}{client.Idletime}");
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

        private static async Task<Sta[]> GetConnectedClients(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig, ILogger logger)
        {
            var controllerClient = new UnifiControllerClient(GetHttpClient(controllerConfig.BaseUrl, presenceConfig.SOCKS, presenceConfig.VerifySsl));

            var loginResult = await controllerClient.Login(controllerConfig.UserName, controllerConfig.Password);
            if (loginResult == true)
            {
                var sites = await controllerClient.GetSites();
                string siteName = null;
                if (string.IsNullOrEmpty(controllerConfig.ControllerSiteDescription) || controllerConfig.ControllerSiteDescription?.ToLower() == "default")
                {
                    siteName = sites.FirstOrDefault(p => string.Compare("default", p.Name, StringComparison.InvariantCultureIgnoreCase) == 0)?.Name;
                }
                else
                {
                    siteName =sites.FirstOrDefault(p => string.Compare(controllerConfig.ControllerSiteDescription, p.Desc, StringComparison.InvariantCultureIgnoreCase) == 0)?.Name;
                }
                if (siteName == null)
                {
                    logger.LogError($"The controller site description {controllerConfig.ControllerSiteDescription} could not be found.");
                }
                else
                {
                    var connectedDevices = await controllerClient.GetConnectedClients(siteName);
                    return connectedDevices;
                }
            }
            else { logger.LogError("Failed to log on to the controller."); }

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
        private static string GetCameraDescription(Camera camera)
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
