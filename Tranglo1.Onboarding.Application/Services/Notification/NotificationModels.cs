using Newtonsoft.Json;

namespace Tranglo1.Onboarding.Application.Services.Notification
{
    internal class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    internal class EmailRequestResponse
    {
        [JsonProperty("requetId")]
        public string RequetId { get; set; }
    }
}
