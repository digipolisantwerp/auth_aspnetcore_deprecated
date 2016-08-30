using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Digipolis.Auth
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

            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            return await httpClient.PostAsync(requestUri, stringContent); 
        }
    }
}
