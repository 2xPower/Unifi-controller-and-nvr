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
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets<Program>()
                    .Build();

            var nvrConfig = config.GetSection("nvr").Get<NvrConfig>();
            var controllerConfig = config.GetSection("controller").Get<ControllerConfig>();
            var presenceConfig = config.GetSection("presence").Get<PresenceRecordingSettings>();

            bool isPresenceDetected = IsOneOrMoreMACPresent(controllerConfig, presenceConfig).Result;
            Console.WriteLine($"Is precense detected: {isPresenceDetected}");
            UpdateCameraRecordingState(nvrConfig, presenceConfig, isPresenceDetected).GetAwaiter().GetResult();

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

        }

        private static async Task<bool> IsOneOrMoreMACPresent(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig)
        {
            var controllerClient = new UnifiControllerClient(GetHttpClient(controllerConfig.BaseUrl, controllerConfig.SocksProxy, controllerConfig.VerifySsl));
            var loginResult = await controllerClient.Login(controllerConfig.UserName, controllerConfig.Password);
            var siteName = controllerClient.GetSites().Result.First(p => string.Compare(controllerConfig.ControllerSiteDescription, p.Desc, StringComparison.InvariantCultureIgnoreCase) == 0).Name;

            var connectedDevices = await controllerClient.GetConnectedClients(siteName);

            var precenseIndicatingDevices = connectedDevices.Where(a => presenceConfig.PresenceIndicationMACs.Any(b => string.Compare(b, a.Mac, StringComparison.InvariantCultureIgnoreCase) == 0)).ToArray();

            if(precenseIndicatingDevices?.Any() == true)
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
