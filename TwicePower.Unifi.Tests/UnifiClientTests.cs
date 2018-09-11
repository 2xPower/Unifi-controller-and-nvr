using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using Xunit;

namespace TwicePower.Unifi.Tests
{
    public class UnifiClientTests
    {
        readonly ControllerConfig config;
        readonly ControllerConfig configWithInvalidSsl;
        public UnifiClientTests()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<UnifiClientTests>();
            var configRoot = builder.Build();
            config = configRoot.GetSection("controller").Get<ControllerConfig>();
            configWithInvalidSsl = configRoot.GetSection("controllerWithInvalidSslCertificate").Get<ControllerConfig>();
        }

        public HttpClient GetHttpClient(string baseUrl, string socks = null,  bool sslVerify = true)
        {
            SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
            {
                UseProxy = !string.IsNullOrEmpty(socks),
                Proxy = new WebProxy(socks, false)
            };

            if (!sslVerify)
            {
                socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback += (a, b, c, d) => true;
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

        [Fact]
        public async Task Login_Succeeds_SslverifyFalse()
        {
            //needs a localhost socks proxy (like fiddler)
            HttpClient httpClient = GetHttpClient(configWithInvalidSsl.BaseUrl, configWithInvalidSsl.SocksProxy, configWithInvalidSsl.VerifySsl);
            var uc = new UnifiControllerClient(httpClient);
            await uc.Login(config.UserName, config.Password);
        }
    
   
        [Fact]
        public async Task Login_Succeeds_nginx()
        {
            HttpClient httpClient = GetHttpClient(config.BaseUrl);
            var uc = new UnifiControllerClient(httpClient);
            await uc.Login(config.UserName, config.Password);
        }

        [Fact]
        public async Task Login_Succeeds_Ubnt_demo()
        {
            HttpClient httpClient = GetHttpClient("https://demo.ubnt.com/");
            var uc = new UnifiControllerClient(httpClient);
            await uc.Login("demo", "demo");
        }


        [Fact]
        public async Task GetConnectedClients()
        {
            HttpClient httpClient = GetHttpClient(config.BaseUrl);
            var uc = new UnifiControllerClient(httpClient);
            await uc.Login(config.UserName, config.Password);
            var actual = await uc.GetConnectedClients();
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task GetSysInfo()
        {
            HttpClient httpClient = GetHttpClient(config.BaseUrl);
            var uc = new UnifiControllerClient(httpClient);
            await uc.Login(config.UserName, config.Password);
            var actual = await uc.GetSysInfo();
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task GetUserGroups()
        {
            HttpClient httpClient = GetHttpClient(config.BaseUrl);
            var uc = new UnifiControllerClient(httpClient);
            await uc.Login(config.UserName, config.Password);
            var actual = await uc.GetUserGroups();
            Assert.NotNull(actual);
        }

    }
}
