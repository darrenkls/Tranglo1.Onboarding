using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
using Tranglo1.Onboarding.Application.DTO.EmailNotification;

namespace Tranglo1.Onboarding.Application.Services.Notification
{
	internal class NotificationService : INotificationService
	{
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IConfiguration _config;
		private IHostEnvironment _env;
		//this is a test comment
		public NotificationService(
			IHttpClientFactory _httpClientFactory, 
			IConfiguration config,
			IHostEnvironment env)
		{
			httpClientFactory = _httpClientFactory;
			_config = config;
			_env = env;
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

				using (var client = this.httpClientFactory.CreateClient())
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

		public async Task<Result<HttpStatusCode>> SendNotification(string to, string subject, string body, string userName, string redirectUri, NotificationTypes notificationTypes, NotificationTemplate? notificationTemplates)
		{
			var getAccessTokenResponse = await GetAccessToken();

			if (getAccessTokenResponse.IsFailure)
			{
				return Result.Failure<HttpStatusCode>(getAccessTokenResponse.Error);
			}

			SendNotification sendNotification = new SendNotification
			{
				To = to,
				Subject = subject,
				Body = body,
				UserName = userName,
				RedirectUri = redirectUri,
				NotificationTypes = notificationTypes,
				NotificationTemplates = notificationTemplates
			};

			var jSonData = JsonConvert.SerializeObject(sendNotification);

			using (var client = this.httpClientFactory.CreateClient())
			{
				client.BaseAddress = new Uri(_config.GetValue<string>("NotificationServiceUri"));

				try
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getAccessTokenResponse.Value);

					using (var response =
						await client.PostAsync("api/Notification/SendNotification", new StringContent(jSonData, Encoding.UTF8, "application/json")))
					{

						if (response.IsSuccessStatusCode)
						{
							return Result.Success<HttpStatusCode>(response.StatusCode);
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

		public async Task<Result<HttpStatusCode>> SendNotification(List<RecipientsInputDTO> recipients, List<RecipientsInputDTO> bcc, List<RecipientsInputDTO> cc, List<IFormFile> attachments, string subject, string body, NotificationTypes notificationTypes, NotificationTemplate notificationTemplate = null)
		{
			var getAccessTokenResponse = await GetAccessToken();

			if (getAccessTokenResponse.IsFailure)
			{
				return Result.Failure<HttpStatusCode>(getAccessTokenResponse.Error);
			}


			EmailNotificationInputDTO emailNotificationInputDTOs = new EmailNotificationInputDTO
			{
				recipients = recipients,
				bcc = bcc,
				cc = cc,
				subject = _env.EnvironmentName.ToLower() == Environments.Production.ToLower() ? subject : $"[{_env.EnvironmentName}]" + subject,
				body = body,

			};

			var jSonData = JsonConvert.SerializeObject(emailNotificationInputDTOs);

			var res = new EmailRequestResponse();

			using (var client = this.httpClientFactory.CreateClient())
			{
				client.BaseAddress = new Uri(_config.GetValue<string>("NotificationServiceUri"));

				try
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getAccessTokenResponse.Value);


					var response = await client.PostAsync("api/emails", new StringContent(jSonData, Encoding.UTF8, "application/json"));

					var requestid = await response.Content.ReadAsStringAsync();

					res = JsonConvert.DeserializeObject<EmailRequestResponse>(requestid);
					using var form = new MultipartFormDataContent();
					if (attachments != null) { 				
						foreach (var formFile in attachments)
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
					using (var response2 = await client.PostAsync($"api/emails/{res.RequetId}/attachments", form)){ }
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

        public async Task<Result<HttpStatusCode>> SendNotification(EmailNotificationInputDTO emailNotificationInputDTO)
        {
            var getAccessTokenResponse = await GetAccessToken();

            if (getAccessTokenResponse.IsFailure)
            {
                return Result.Failure<HttpStatusCode>(getAccessTokenResponse.Error);
            }

            emailNotificationInputDTO.subject = _env.EnvironmentName.ToLower() == Environments.Production.ToLower() ? emailNotificationInputDTO.subject : $"[{_env.EnvironmentName}]" + emailNotificationInputDTO.subject;

            var jSonData = JsonConvert.SerializeObject(emailNotificationInputDTO);

            using (var client = this.httpClientFactory.CreateClient())
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

        //public Result<string> GetTemplate(NotificationTemplate request)
        //{
        //	if (request == NotificationTemplate.SignUpVerification)
        //	{
        //		string FilePathVerifyEmail = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\VerifyEmailTemplate.html";
        //		StreamReader strVerifyEmail = new StreamReader(FilePathVerifyEmail);
        //		string MailTextVerifyEmail = strVerifyEmail.ReadToEnd();
        //		strVerifyEmail.Close();

        //		return Result.Success(MailTextVerifyEmail);
        //	}			
        //	else if (request == NotificationTemplate.SignUpSuccessful)
        //	{

        //	}
        //	else if (request == NotificationTemplate.PasswordReset)
        //	{
        //		string FilePathResetPassword = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\PasswordResetTemplate.html";
        //		StreamReader strResetPassword = new StreamReader(FilePathResetPassword);
        //		string MailTextResetPassword = strResetPassword.ReadToEnd();
        //		strResetPassword.Close();

        //		return Result.Success(MailTextResetPassword);
        //	}
        //	else if (request == NotificationTemplate.InviteUserExistingUser)
        //	{
        //		string FilePathInviteUserExistingUser = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\InviteUserExistingUserTemplate.html";
        //		StreamReader strInviteUserExistingUser = new StreamReader(FilePathInviteUserExistingUser);
        //		string MailTextInviteUserExistingUser = strInviteUserExistingUser.ReadToEnd();
        //		strInviteUserExistingUser.Close();

        //		return Result.Success(MailTextInviteUserExistingUser);
        //	}
        //	else if (request == NotificationTemplate.InviteUserNewUser)
        //	{
        //		string FilePathInviteUserNewUser = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\InviteUserNewUserTemplate.html";
        //		StreamReader strInviteUserNewUser = new StreamReader(FilePathInviteUserNewUser);
        //		string MailTextInviteUserNewUser = strInviteUserNewUser.ReadToEnd();
        //		strInviteUserNewUser.Close();

        //		return Result.Success(MailTextInviteUserNewUser);
        //	}
        //	else if( request == NotificationTemplate.DocumentReleased)
        //          {
        //		string FilePathDocumentReleased = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\DocumentReleasedTemplate.xslt";
        //			StreamReader strDocumentReleased = new StreamReader(FilePathDocumentReleased);
        //			string MailTextDocumentReleased = strDocumentReleased.ReadToEnd();
        //			strDocumentReleased.Close();
        //			return Result.Success(MailTextDocumentReleased);
        //	}
        //	else if (request == NotificationTemplate.SubmissionReview)
        //	{
        //		string FilePathSubmissionReview = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\SubmissionforReviewTemplate.xslt";
        //		StreamReader strSubmissionReview = new StreamReader(FilePathSubmissionReview);
        //		string MailTextSubmissionReview = strSubmissionReview.ReadToEnd();
        //		strSubmissionReview.Close();
        //		return Result.Success(MailTextSubmissionReview);
        //	}
        //	else if (request == NotificationTemplate.ResubmissionReview)
        //	{
        //		string FilePathResubmissionReview = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\ResubmissionforReviewTemplate.xslt";
        //		StreamReader strResubmissionReview = new StreamReader(FilePathResubmissionReview);
        //		string MailTextResubmissionReview = strResubmissionReview.ReadToEnd();
        //		strResubmissionReview.Close();
        //		return Result.Success(MailTextResubmissionReview);
        //	}
        //	else if (request == NotificationTemplate.IncompleteReviewResult)
        //	{
        //		string FilePathIncomplteReviewResult = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\IncompleteReviewResultTemplate.xslt";
        //		StreamReader strIncompleteReviewResult = new StreamReader(FilePathIncomplteReviewResult);
        //		string MailTextIncompleteReviewResult = strIncompleteReviewResult.ReadToEnd();
        //		strIncompleteReviewResult.Close();
        //		return Result.Success(MailTextIncompleteReviewResult);
        //	}
        //	else if (request == NotificationTemplate.PendingReviewResult)
        //	{
        //		string FilePathPendingReviewResult = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\PendingReviewResultTemplate.xslt";
        //		StreamReader strPendingReviewResult = new StreamReader(FilePathPendingReviewResult);
        //		string MailTextPendingReviewResult = strPendingReviewResult.ReadToEnd();
        //		strPendingReviewResult.Close();
        //		return Result.Success(MailTextPendingReviewResult);
        //	}
        //	else if (request == NotificationTemplate.RejectReviewResult)
        //	{
        //		string FilePathRejectReviewResult = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\RejectReviewResultTemplate.xslt";
        //		StreamReader strRejectReviewResult = new StreamReader(FilePathRejectReviewResult);
        //		string MailTextRejectReviewResult = strRejectReviewResult.ReadToEnd();
        //		strRejectReviewResult.Close();
        //		return Result.Success(MailTextRejectReviewResult);
        //	}
        //	else if (request == NotificationTemplate.ApproveReviewResult)
        //	{
        //		string FilePathApproveReviewResult = $"{_env.ContentRootPath}\\wwwroot\\Templates\\EmailTemplate\\ApproveReviewResultTemplate.xslt";
        //		StreamReader strApproveReviewResult = new StreamReader(FilePathApproveReviewResult);
        //		string MailTextApproveReviewResult = strApproveReviewResult.ReadToEnd();
        //		strApproveReviewResult.Close();
        //		return Result.Success(MailTextApproveReviewResult);
        //	}

        //	return Result.Failure<string>($"Notification Template {request} not found");

        //}
    }
}
