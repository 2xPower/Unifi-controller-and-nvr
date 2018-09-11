using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwicePower.Unifi.Nvr;

namespace TwicePower.Unifi
{
    public class UnifiVideoClient
    {
        readonly HttpClient httpClient;

        public UnifiVideoClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<bool> Login(string userName, string password)
        {
            var postContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new { password = password, username = userName }), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("/api/2.0/login", postContent);
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<Status> GetStatus()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/api/2.0/bootstrap");
            response.EnsureSuccessStatusCode();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<NvrApiResult<Status>>(await response.Content.ReadAsStringAsync());
            return result.Data[0];

        }
        public async Task<bool> UpdateCamera(Camera camera)
        {
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/2.0/camera/{camera.Id}", camera);
            response.EnsureSuccessStatusCode();
            return true;
        }

    }
}
