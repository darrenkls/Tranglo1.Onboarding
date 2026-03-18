using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler.Notification
{
    public class NotificationService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<Result<HttpStatusCode>> SendNotification(SendNotificationInput request, List<IFormFile> attachments)
        {
            var getAccessTokenResponse = await GetAccessToken();

            if (getAccessTokenResponse.IsFailure)
            {
                return Result.Failure<HttpStatusCode>(getAccessTokenResponse.Error);
            }

            string env = _config.GetValue<string>("Environment");

            SendNotificationInput emailNotificationInputDTOs = new SendNotificationInput
            {
                recipients = request.recipients,
                bcc = request.bcc,
                cc = request.cc,
                subject = (env.ToLower() == "Production".ToLower() || string.IsNullOrEmpty(env)) ? request.subject : $"[{env}]" + request.subject,
                body = request.body,
                Module = String.IsNullOrWhiteSpace(request.Module) ? "KYC Email" : request.Module,
                SubModule = request.SubModule
            };

            var jSonData = JsonConvert.SerializeObject(emailNotificationInputDTOs);

            var res = new EmailRequestResponse();

            using (var client = this._httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(_config.GetValue<string>("NotificationServiceUri"));

                try
                {
                    //var accessToken = await _httpContextAcessor.HttpContext.GetTokenAsync("access_token");
                    var accessToken = getAccessTokenResponse.Value;


                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                    var response = await client.PostAsync("api/emails", new StringContent(jSonData, Encoding.UTF8, "application/json"));
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Result.Failure<HttpStatusCode>($"Error: {response.StatusCode}. Content: {errorContent}");

                    }
                    var requestid = await response.Content.ReadAsStringAsync();

                    res = JsonConvert.DeserializeObject<EmailRequestResponse>(requestid);
                    using var form = new MultipartFormDataContent();
                    if (attachments != null)
                    {
                        foreach (var formFile in attachments)
                        {
                            if (formFile.Length > 0)
                            {
                                using (var memoryStream = new MemoryStream())
                                {

                                    //Get the file steam from the multiform data uploaded from the browser
                                    await formFile.CopyToAsync(memoryStream);
                                    memoryStream.Position = 0; // Reset position after copying
                                    var fileContent = new ByteArrayContent(memoryStream.ToArray());
                                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                                    form.Add(fileContent, "files", formFile.FileName);
                                }

                            }
                        };
                    }
                    using (var response2 = await client.PostAsync($"api/emails/{res.RequetId}/attachments", form)) { }
                    HttpContent c = new StringContent("{}", Encoding.UTF8, "application/json");

                    var uri = "api/emails/" + res.RequetId + "/send";

                    using (var response2 = await client.PostAsync(uri, c))
                    {
                        if (response2.IsSuccessStatusCode)
                        {
                            return Result.Success<HttpStatusCode>(response2.StatusCode);
                        }

                        return Result.Failure<HttpStatusCode>("Send notification failed.");
                    }
                }
                catch (Exception ex)
                {
                    return Result.Failure<HttpStatusCode>(ex.ToString());
                }
            }
        }

        private async Task<Result<string>> GetAccessToken()
        {
            try
            {
                NotificationServiceAuthentication notificationServiceAuthentication = _config.GetSection("NotificationServiceAuthentication").Get<NotificationServiceAuthentication>();

                FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    ["client_id"] = notificationServiceAuthentication.client_id,
                    ["client_secret"] = notificationServiceAuthentication.client_secret,
                    ["scope"] = notificationServiceAuthentication.scope,
                    ["grant_type"] = notificationServiceAuthentication.grant_type
                });

                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
                {
                    CharSet = "UTF-8"
                };

                using (var client = this._httpClientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri(_config.GetValue<string>("IdentityServerUri"));

                    using (HttpResponseMessage response = await client.PostAsync("connect/token", content))
                    {
                        using (var data = response.Content)
                        {
                            var result = await data.ReadAsStringAsync();
                            var deserializeResponse = JsonConvert.DeserializeObject<TokenResponse>(result);

                            if (deserializeResponse != null && deserializeResponse.Error != null)
                            {
                                return Result.Failure<string>(deserializeResponse.Error);
                            }

                            return Result.Success(deserializeResponse.AccessToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Failure<string>(ex.ToString());
            }
        }


        public async Task<Result<HttpStatusCode>> SendNotification(EmailNotificationInputDTO emailNotificationInputDTO)
        {
            var getAccessTokenResponse = await GetAccessToken();

            if (getAccessTokenResponse.IsFailure)
            {
                return Result.Failure<HttpStatusCode>(getAccessTokenResponse.Error);
            }

            string env = _config.GetValue<string>("Environment");
            emailNotificationInputDTO.subject = (env.ToLower() == "Production".ToLower() || string.IsNullOrEmpty(env)) ? emailNotificationInputDTO.subject : $"[{env}]" + emailNotificationInputDTO.subject;

            var jSonData = JsonConvert.SerializeObject(emailNotificationInputDTO);

            using (var client = this._httpClientFactory.CreateClient())
            {
                client.BaseAddress = new Uri(_config.GetValue<string>("NotificationServiceUri"));

                try
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getAccessTokenResponse.Value);

                    using var form = new MultipartFormDataContent();

                    // Add Email Body Info
                    form.Add(new StringContent(jSonData, Encoding.UTF8, "application/json"), "emailData");

                    // Add Attachments
                    if (emailNotificationInputDTO?.Attachments != null && emailNotificationInputDTO?.Attachments.Count() > 0)
                    {
                        foreach (var formFile in emailNotificationInputDTO.Attachments)
                        {
                            if (formFile.Length > 0)
                            {
                                using (var memoryStream = new MemoryStream())
                                {

                                    //Get the file steam from the multiform data uploaded from the browser
                                    await formFile.CopyToAsync(memoryStream);

                                    var fileContent = new ByteArrayContent(memoryStream.ToArray());
                                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                                    form.Add(fileContent, "files", formFile.FileName);
                                }
                            }
                        };
                    }

                    var uri = "api/emails/sendEmailAttachments";

                    using (var response2 = await client.PostAsync(uri, form))
                    {
                        if (response2.IsSuccessStatusCode)
                        {
                            return Result.Success<HttpStatusCode>(response2.StatusCode);
                        }

                        return Result.Failure<HttpStatusCode>("Send notification failed.");
                    }
                }
                catch (Exception ex)
                {
                    return Result.Failure<HttpStatusCode>(ex.ToString());
                }
            }
        }

    }
}
