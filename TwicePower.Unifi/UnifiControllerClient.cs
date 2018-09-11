using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TwicePower.Unifi.Controller;

namespace TwicePower.Unifi
{
    public class UnifiControllerClient
    {
        readonly HttpClient httpClient;

        public UnifiControllerClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<bool> Login(string userName, string password)
        {
            var postContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new { password = password, remember = true, strict = true, username = userName }), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("/api/login", postContent);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<Site[]> GetSites()
        {
            var result = await httpClient.GetAsync($"/api/self/sites");
            result.EnsureSuccessStatusCode();
            var unifiApiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<UnifiApiResult<Site[]>>(await result.Content.ReadAsStringAsync());
            //unifiApiResult.EnsureUnifiApiResultOk();
            return unifiApiResult.Data;

        }

        public async Task<Sta[]> GetConnectedClients(string site = "default")
        {
            var result = await httpClient.GetAsync($"/api/s/{site}/stat/sta");
            result.EnsureSuccessStatusCode();
            var unifiApiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<UnifiApiResult<Sta[]>>(await result.Content.ReadAsStringAsync());
            //unifiApiResult.EnsureUnifiApiResultOk();
            return unifiApiResult.Data;
        }

        public async Task<SysInfo> GetSysInfo(string site = "default")
        {
            var result = await httpClient.GetAsync($"/api/s/{site}/stat/sysinfo");
            result.EnsureSuccessStatusCode();
            var unifiApiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<UnifiApiResult<SysInfo[]>>(await result.Content.ReadAsStringAsync());
            //unifiApiResult.EnsureUnifiApiResultOk();
            return unifiApiResult.Data[0];   
        }

        public async Task<UserGroup[]> GetUserGroups(string site = "default")
        {
            var result = await httpClient.GetAsync($"/api/s/{site}/rest/usergroup");
            result.EnsureSuccessStatusCode();
            var unifiApiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<UnifiApiResult<UserGroup[]>>(await result.Content.ReadAsStringAsync());
            //unifiApiResult.EnsureUnifiApiResultOk();
            return unifiApiResult.Data;
        }

        void EnsureUnifiApiResultOk<T>(UnifiApiResult<T> unifiApiResult)
        {
            if(unifiApiResult == null)
            {
                throw new Exception("UnifiApiResult is null");
            }
            if (unifiApiResult.Meta == null)
            {
                throw new Exception("UnifiApiResult.Meta is null");
            }
            if(string.Compare(unifiApiResult.Meta.Rc, "ok") != 0)
            {
                throw new Exception($"UnifiApiResult.Meta.Rs value is {unifiApiResult.Meta.Rc}");
            }
        }
    }

}
