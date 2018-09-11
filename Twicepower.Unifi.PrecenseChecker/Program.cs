using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TwicePower.Unifi.PrecenseChecker
{
    class Program
    {
        public static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            CommandArgument names = null;
            var linesplit = $"{Environment.NewLine}\t\t\t\t\t ";
            commandLineApplication.Command("list-clients",
              (target) =>
                names = target. Argument(
                  "list connected wireless clients",
                  "Enter the full name of the person to be greeted.")); 

            CommandOption userNameController = commandLineApplication.Option("-uc |--username-controller <username>",
                $"The user name  for unifi controller. If username-nvr is not specified,{linesplit}this value will be used for controller and nvr.{linesplit}If not for the controller only", CommandOptionType.SingleValue);
            CommandOption passWordController = commandLineApplication.Option("-pc |--password-controller <password>",
               $"The password for unifi controller. If password-nvr  is not specified,{linesplit}this value will be used for controller and nvr.{linesplit}If not for the controller only", CommandOptionType.SingleValue);
            CommandOption baseUrlController = commandLineApplication.Option("-urlc | --url-controller <url>",
               $"The url for the Unif Controller.", CommandOptionType.SingleValue);

            CommandOption userNameNvr = commandLineApplication.Option("-un | --username-nvr <username>",
               "The user name  for unifi Nvr.", CommandOptionType.SingleValue);
            CommandOption passWordNvr = commandLineApplication.Option("-pn | --password-nvr <password>",
               "The password for unifi controller.", CommandOptionType.SingleValue);
            CommandOption baseUrlNvr = commandLineApplication.Option("-urln | --url-nvr <url>",
               "The url for the Ubiquity Video - NVR.", CommandOptionType.SingleValue);



            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                if (userNameController.HasValue())
                {

                }
                return 0;
            });
            commandLineApplication.Execute(args);
            #region Config
            IConfigurationRoot config = LoadConfigurationFromFile();

            var nvrConfig = config.GetSection("nvr").Get<NvrConfig>();
            var controllerConfig = config.GetSection("controller").Get<ControllerConfig>();
            var presenceConfig = config.GetSection("presence").Get<PresenceRecordingSettings>();

            // configuration check
            Console.WriteLine($"{nameof(controllerConfig)} is null: {controllerConfig == null}");
            Console.WriteLine($"{nameof(controllerConfig)} base url: {controllerConfig.BaseUrl}");

            Console.WriteLine($"{nameof(nvrConfig)} is null: {nvrConfig == null}");
            Console.WriteLine($"{nameof(nvrConfig)} base url: {nvrConfig.BaseUrl}");
            Console.WriteLine($"{nameof(presenceConfig)} is null: {presenceConfig == null}"); 
            #endregion

            bool isPresenceDetected = IsOneOrMoreMACPresent(controllerConfig, presenceConfig).Result;

            Console.WriteLine($"Is precense detected: {isPresenceDetected}");

            UpdateCameraRecordingState(nvrConfig, presenceConfig, isPresenceDetected).GetAwaiter().GetResult();

        }

        private static IConfigurationRoot LoadConfigurationFromFile()
        {
            var installedPath = new FileInfo(typeof(Program).Assembly.CodeBase).Directory.FullName;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                if (installedPath.Contains(':'))
                {
                    installedPath = installedPath.Split(':')[1];
                }
            }
            else
            {
                var indexOfFile = installedPath.IndexOf("file:");
                if (indexOfFile > 0)
                {
                    installedPath = new Uri(installedPath.Substring(indexOfFile)).AbsolutePath;
                }

            }

            var configFilePath = Path.Combine(installedPath, "appsettings.json");

            Console.WriteLine($"using config at {configFilePath}");
            if (!File.Exists(configFilePath))
                throw new Exception("appsettings.json not found");

            var config = new ConfigurationBuilder()
                    .SetBasePath(installedPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets<Program>()
                    .Build();
            return config;
        }

        private static async Task<bool> IsOneOrMoreMACPresent(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig)
        {
            var controllerClient = new UnifiControllerClient(GetHttpClient(controllerConfig.BaseUrl, controllerConfig.SocksProxy, controllerConfig.VerifySsl));

            var loginResult = await controllerClient.Login(controllerConfig.UserName, controllerConfig.Password);
            if (loginResult == true)
            {
                var siteName = controllerClient.GetSites().Result.First(p => string.Compare(controllerConfig.ControllerSiteDescription, p.Desc, StringComparison.InvariantCultureIgnoreCase) == 0).Name;

                var connectedDevices = await controllerClient.GetConnectedClients(siteName);
                if (connectedDevices?.Count() > 0)
                {

                    var precenseIndicatingDevices = connectedDevices.Where(a => presenceConfig.PresenceIndicationMACs.Any(b => string.Compare(b, a.Mac, StringComparison.InvariantCultureIgnoreCase) == 0)).ToArray();

                    if (precenseIndicatingDevices?.Any() == true)
                    {
                        foreach (var device in precenseIndicatingDevices)
                        {
                            Console.WriteLine($"{device.Hostname} is connected.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("None of the configured MAC are connected.");
                    }
                    return precenseIndicatingDevices?.Any() == true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("Failed to log on to the controller");
            }

            
        }

        private static async Task UpdateCameraRecordingState(NvrConfig nvrConfig,PresenceRecordingSettings presenceConfig, bool isPresenceDetected)
        {
            var nvrClient = new TwicePower.Unifi.UnifiVideoClient(GetHttpClient(nvrConfig.BaseUrl, nvrConfig.SocksProxy, true));

            if (await nvrClient.Login(nvrConfig.UserName, nvrConfig.Password))
            {
                var status = await nvrClient.GetStatus();
                foreach (var cameraId in presenceConfig.CameraIdsToSetToMotionRecordingIfNoOneIsPresent)
                {
                    var camera = status.Cameras.FirstOrDefault(p => p.Id == cameraId);
                    Console.WriteLine($"Camera {camera.Name} is recording motion: {camera.RecordingSettings.MotionRecordEnabled}");
                    if (camera != null && camera.RecordingSettings?.MotionRecordEnabled != !isPresenceDetected)
                    {
                        Console.WriteLine($"Updating camera {camera.Name}");
                        camera.RecordingSettings.MotionRecordEnabled = !isPresenceDetected;
                        await nvrClient.UpdateCamera(camera);
                    }
                    else
                    {
                        Console.WriteLine($"Update for camera not required.");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Failed to log in to NVR");
            }

        }

        private static HttpClient GetHttpClient(string baseUrl, string socksProxy = null, bool sslVerify = true)
        {
            SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler();
            if(!string.IsNullOrWhiteSpace(socksProxy) && Uri.IsWellFormedUriString(socksProxy, UriKind.Absolute))
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
