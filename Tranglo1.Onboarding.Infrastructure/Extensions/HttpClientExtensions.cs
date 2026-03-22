using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Infrastructure.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient httpClient,
            string requestUri,
            T value)
        {
            var json = JsonConvert.SerializeObject(value);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await httpClient.PostAsync(requestUri, content);
        }

        public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(
            this HttpClient httpClient,
            string requestUri,
            T value)
        {
            var json = JsonConvert.SerializeObject(value);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await httpClient.PutAsync(requestUri, content);
        }

        public static async Task<T> ReadFromJsonAsync<T>(this HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(json);
            }

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
