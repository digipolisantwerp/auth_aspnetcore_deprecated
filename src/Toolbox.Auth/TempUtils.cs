using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Toolbox.Auth
{
    public static class TempUtils
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var response = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(response);
        }

        public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient httpClient, string requestUri, T content, JsonSerializerSettings jsonSettings)
        {
            var json = JsonConvert.SerializeObject(content, jsonSettings);

            var stringContent = new StringContent(json);

            return await httpClient.PostAsync(requestUri, stringContent); 
        }
    }
}
