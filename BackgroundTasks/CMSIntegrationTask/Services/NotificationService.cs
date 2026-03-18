using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;

namespace CMSIntegrationTask.Services
{
	public class NotificationService
	{
		private IConfiguration Configuration;
		private string _correlationID;

		public NotificationService(IConfiguration configuration)
		{
			Configuration = configuration;
			_correlationID = Guid.NewGuid().ToString();
		}

		public NotificationService(IConfiguration configuration, string correlationID)
		{
			Configuration = configuration;
			_correlationID = correlationID; 
		}

		private class OAuthCredentials
		{
			public string Client_id { get; set; }
			public string Client_secret { get; set; }
			public string Scope { get; set; }
			public string Grant_type { get; set; }
		}

		private async Task<Result<string>> GetAccessTokenAsync()
		{
			var oauthCred = new OAuthCredentials();
			Configuration.GetSection("OAuthClientAuthentication").Bind(oauthCred);

			
			FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["client_id"] = oauthCred.Client_id,
				["client_secret"] = oauthCred.Client_secret,
				["scope"] = oauthCred.Scope,
				["grant_type"] = oauthCred.Grant_type
			});
			content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
			{
				CharSet = "UTF-8"
			};
			using var httpClient = new HttpClient();

			httpClient.BaseAddress = new Uri(Configuration["IdentityServerUri"]);

			using HttpResponseMessage response = await httpClient.PostAsync("connect/token", content);
			using var data = response.Content;

			var result = await data.ReadAsStringAsync();
			var deserializeResponse = JsonConvert.DeserializeObject<TokenResponse>(result);

			if (deserializeResponse != null && deserializeResponse.Error != null)
			{
				Log.Error($"{_correlationID} GetAccessTokenAsync {deserializeResponse.Error}");

				return Result.Failure<string>(deserializeResponse.Error);
			}

			return Result.Success(deserializeResponse.AccessToken);
		}

		public async Task<Result<bool>> SendNotificationAsync(PartnerSubscription subscription)
		{
			using var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", _correlationID);
			httpClient.DefaultRequestHeaders.Add("X-T1-API-Key", Configuration["T1ApiKey"]);

			try
			{
				string param = $"partnerSubscriptionCode={subscription.Id}";

				httpClient.BaseAddress = new Uri(Configuration["IdentityServerUri"]);
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var httpContent = new StringContent(JsonConvert.SerializeObject(subscription.Id), Encoding.UTF8, "application/json");

				// NOTE: Refactor backgroundService endpoint from PartnerCode to PartnerSubscriptionCode. Temporarily set to null
				var httpResponse = await httpClient.PostAsync($"/api/v1/backgroundService/{subscription.PartnerCode}/go-live-notification?" + param, httpContent);
				
				if (httpResponse.IsSuccessStatusCode)
				{
					return Result.Success(true);
				}
				else
				{
					using var data = httpResponse.Content;
					var result = await data.ReadAsStringAsync();

					Log.Error($"{_correlationID} SendNotificationAsync Failed to send notification");
					Log.Error($"{_correlationID} SendNotificationAsync go-live-notification: {httpResponse.StatusCode}");
					Log.Error($"{_correlationID} SendNotificationAsync go-live-notification: {result}");

					return Result.Failure<bool>("Failed to send notification");
				}
			}
			catch(Exception ex)
			{
				Log.Error($"{_correlationID} SendNotificationAsync Exception {ex.Message}");
				Log.Error($"{_correlationID} SendNotificationAsync StackTrace {ex.StackTrace}");

				return Result.Failure<bool>($"{ex}");

			}

		}
	}
}
