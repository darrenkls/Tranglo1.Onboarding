using CMSIntegrationTask.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace CMSIntegrationTask.Services
{
	class WalletManager
	{
		public WalletManager(HttpClient httpClient, IPartnerRepository partnerRepository, 
			NotificationService notificationService, ICorrelationIdProvider correlationIdProvider,
			IUnitOfWork unitOfWork, ILogger<WalletManager> logger)
		{
			HttpClient = httpClient;
			PartnerRepository = partnerRepository;
			NotificationService = notificationService;
			CorrelationIdProvider = correlationIdProvider;
			UnitOfWork = unitOfWork;
			Logger = logger;
		}

		public HttpClient HttpClient { get; }
		public IPartnerRepository PartnerRepository { get; }
		public NotificationService NotificationService { get; }
		public ICorrelationIdProvider CorrelationIdProvider { get; }
		public IUnitOfWork UnitOfWork { get; }
		public ILogger<WalletManager> Logger { get; }

		public async Task ProcessWalletsAsync()
		{
			try
			{
				var apiName = "/WalletIntegration/GetRequisitionStatus";

				//0 means?
				var pendingWallet = await PartnerRepository.GetPartnerWalletListByStatus(((int)CMSStatus.Pending).ToString());
				Logger.LogInformation($"Pending Wallet Count: {pendingWallet.Count()}");

				//httpClient.DefaultRequestHeaders.Add("Authorization", $"api {CMSApiKey}");
				//httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				//httpClient.BaseAddress = new Uri(CMSApiUri);

				if (pendingWallet.Count() > 0)
				{
					GetWalletRequisitionStatusInputDto inputDto = new GetWalletRequisitionStatusInputDto()
					{
						Transactions = new List<GetWalletRequisitionStatusInputDto.TransactionDetails>()
					};

					int loop = 0;
					foreach (var i in pendingWallet)
					{
						if (i.PartnerSubscriptionCode == null)
						{
							Logger.LogError($"PartnerSubscriptionCode is Null for PartnerCode : {i.PartnerCode}");
						}
						GetWalletRequisitionStatusInputDto.TransactionDetails transaction = new GetWalletRequisitionStatusInputDto.TransactionDetails()
						{
							RequisitionCode = i.RcCode,
							TrnxID = i.TrnxID,
							T1_Partner_Key = i.PartnerSubscriptionCode.Value
						};
						inputDto.Transactions.Add(transaction);

						Logger.LogInformation($"Pending Transaction Loop Sequence: {loop} " +
												$"\nRequisition Code: {transaction.RequisitionCode} " +
												$"\nTransaction ID: {transaction.TrnxID} " +
												$"\nT1_Partner_Key: {transaction.T1_Partner_Key} ");
						loop++;
					}

					var inputDTOJsonConvert = JsonConvert.SerializeObject(inputDto);

					var httpContent = new StringContent(JsonConvert.SerializeObject(inputDto), Encoding.UTF8, "application/json");
					var httpResponse = await HttpClient.PostAsync(apiName, httpContent);

					Logger.LogInformation($"HttpClient.BaseAddress: {HttpClient.BaseAddress}," +
											$"\nHttpClient.DefaultRequestHeaders: {HttpClient.DefaultRequestHeaders}," +
											$"\nHttpClient.DefaultRequestVersion: {HttpClient.DefaultRequestVersion}");
					Logger.LogInformation($"CMS ApiName: {apiName}");
					Logger.LogInformation($"CMS inputDTOJsonConvert: {inputDTOJsonConvert}");

					var jsonContent = await httpContent.ReadAsStringAsync();
					Logger.LogInformation($"CMS jsonContent: {jsonContent}");

					//var httpResponse = await HttpClient.PostAsJsonAsync(apiName, inputDto);
					var cmsApiResponse = await httpResponse.Content.ReadAsStringAsync();

					Logger.LogInformation($"CMS cmsApiResponse: {cmsApiResponse}");

					if (httpResponse.IsSuccessStatusCode)
					{
						Console.WriteLine(cmsApiResponse);
						Logger.LogInformation($"CMS Response: {cmsApiResponse}");
						//log response
						var cmsResp = JsonConvert.DeserializeObject<GetWalletRequisitionStatusOutputDto>(cmsApiResponse);

						//List<Tranglo1.Onboarding.Domain.Entities.PartnerRegistration> partnerRegistrations = new List<Tranglo1.Onboarding.Domain.Entities.PartnerRegistration>();
						List<Tranglo1.Onboarding.Domain.Entities.PartnerManagement.PartnerSubscription> subscriptions = new List<Tranglo1.Onboarding.Domain.Entities.PartnerManagement.PartnerSubscription>();

						foreach (var t in cmsResp.Result.Where(x => x.Status == ((int)CMSStatus.Approve).ToString()))
						{
							var wallet = pendingWallet.Where(x => x.PartnerSubscriptionCode == t.T1_Partner_Key).FirstOrDefault();
							if (wallet != null)
							{
								wallet.CMSStatus = t.Status;
								//var partnerDetails = await PartnerRepository.GetPartnerDetailsByCodeAsync(t.T1_Partner_Key.Value);
								var subscription = await PartnerRepository.GetSubscriptionAsync(t.T1_Partner_Key.Value);
								subscription.Environment = Tranglo1.Onboarding.Domain.Entities.Environment.Production;
								//await PartnerRepository.UpdatePartnerRegistrationAsync(partnerDetails);
								await PartnerRepository.UpdateSubcriptionAsync(subscription);

								var cmsWalletIntegrationDetails = await PartnerRepository.GetPartnerWalletIntegrationByPartnerSubscriptionCodeAsync(t.T1_Partner_Key.Value);
								cmsWalletIntegrationDetails.CMSStatus = ((int)CMSStatus.Approve).ToString();
								await PartnerRepository.UpdatePartnerWalletIntegration(cmsWalletIntegrationDetails);

								//add subscription to list
								subscriptions.Add(subscription);
							}
						}

						Logger.LogInformation($"CMS Approved T1 Partner ID: {String.Join(", ", subscriptions.Select(x => x.Id))}");

						await UnitOfWork.CommitAsync();

						//send email after commit. IdentityServer would also check if env is production only allow send email
						//Comment Off temporarily
						//foreach (var p in subscriptions)
						//{
						//	//only send email if partner is NOT supply partner
						//	if(p.PartnerType.Id != Tranglo1.Onboarding.Domain.Entities.PartnerType.Supply_Partner.Id)
      //                      {
						//		var notificationResult = await NotificationService.SendNotificationAsync(p);
						//	}
						//}
					}
					else
					{
						Logger.LogWarning($"cmsApiResponse Error {httpResponse.StatusCode}");
						Logger.LogWarning($"cmsApiResponse: {cmsApiResponse}");
					}
				}
				else
				{
					Logger.LogInformation($"No Pending Wallet To Be Processed");
				}
				Logger.LogInformation($"Task completed ");
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.ToString());
			}
		}
	}
}
