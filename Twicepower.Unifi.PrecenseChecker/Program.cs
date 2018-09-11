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
                    if (camera != null && camera.RecordingSettings?.MotionRecordEnabled != !isPresenceDetected)
                    {
                        camera.RecordingSettings.MotionRecordEnabled = !isPresenceDetected;
                        await nvrClient.UpdateCamera(camera);
                    }
                }
            }

        }

        private static async Task<bool> IsOneOrMoreMACPresent(ControllerConfig controllerConfig, PresenceRecordingSettings presenceConfig)
        {
            var controllerClient = new UnifiControllerClient(GetHttpClient(controllerConfig.BaseUrl, controllerConfig.SocksProxy, controllerConfig.VerifySsl));
            var loginResult = await controllerClient.Login(controllerConfig.UserName, controllerConfig.Password);
            var siteName = controllerClient.GetSites().Result.First(p => string.Compare(controllerConfig.ControllerSiteDescription, p.Desc, StringComparison.InvariantCultureIgnoreCase) == 0).Name;

            var connectedDevices = await controllerClient.GetConnectedClients(siteName);

            bool presenceIndicationMacsPresent = connectedDevices.Any(a => presenceConfig.PresenceIndicationMACs.Any(b => string.Compare(b, a.Mac, StringComparison.InvariantCultureIgnoreCase) == 0));
            return presenceIndicationMacsPresent;
        }

        private static HttpClient GetHttpClient(string baseUrl, string socksProxy = null, bool sslVerify = true)
        {
            SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
            {
                UseProxy = !string.IsNullOrWhiteSpace(socksProxy),
                Proxy = new WebProxy(socksProxy, false)
            };

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
