using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCSummary, UACAction.Create)] 
    [Permission(Permission.KYCManagementReviewSummary.Action_Add_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementReviewSummary.Action_View_Code })]
    internal class SaveKYCCustomerSummaryFeedbackCommand : BaseCommand<Result<KYCCustomerSummaryFeedbackOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public KYCCustomerSummaryFeedbackInputDTO KYCCustomerSummaryFeedback { get; set; }
        public string CustomerSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<KYCCustomerSummaryFeedbackOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Save KYC Summary Feedback for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveKYCCustomerSummaryFeedbackCommandHandler : IRequestHandler<SaveKYCCustomerSummaryFeedbackCommand, Result<KYCCustomerSummaryFeedbackOutputDTO>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _businessProfileRepo;
            private readonly ILogger<SaveKYCCustomerSummaryFeedbackCommandHandler> _logger;

            public IUnitOfWork UnitOfWork { get; }

            public SaveKYCCustomerSummaryFeedbackCommandHandler(
                    BusinessProfileService businessProfileService,
                    IBusinessProfileRepository businessProfileRepository,
                    ILogger<SaveKYCCustomerSummaryFeedbackCommandHandler> logger,
                    IUnitOfWork unitOfWork

                )
            {
                _businessProfileService = businessProfileService;
                _businessProfileRepo = businessProfileRepository;
                _logger = logger;
                UnitOfWork = unitOfWork;

            }

            public async Task<Result<KYCCustomerSummaryFeedbackOutputDTO>> Handle(SaveKYCCustomerSummaryFeedbackCommand request, CancellationToken cancellationToken)
            {
                if (String.IsNullOrEmpty(request.KYCCustomerSummaryFeedback.FeedbackToTranglo))
                {
                    return Result.Failure<KYCCustomerSummaryFeedbackOutputDTO>(
                                   $"Feedback form cannot be empty"
                               );
                }

                var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

                var kycCategory = KYCCategory.FindById<KYCCategory>(request.KYCCustomerSummaryFeedback.KYCCategoryCode);

                // Connect Portal
                if (ClaimCode.Connect == request.CustomerSolution)
                {

                    KYCCustomerSummaryFeedback summaryFeedback = new KYCCustomerSummaryFeedback(request.KYCCustomerSummaryFeedback.KYCCustomerSummaryFeedbackCode, businessProfile, kycCategory)
                    {
                        SolutionCode = Solution.Connect.Id,
                        FeedbackToTranglo = request.KYCCustomerSummaryFeedback.FeedbackToTranglo
                    };

                    var result = await _businessProfileRepo.SaveKYCCustomerSummaryFeedback(summaryFeedback);

                    if (result.IsFailure)
                    {
                        return Result.Failure<KYCCustomerSummaryFeedbackOutputDTO>(
                                    $"Save KYC Customer Summary failed for BusinessProfileCode : ${request.BusinessProfileCode}, KYC Category ID: ${kycCategory.Id} "
                                );
                    }


                    KYCCustomerSummaryFeedbackOutputDTO kycCustomerSummaryFeedbackOutputDTO = await UnitOfWork.Connection.QueryFirstAsync<KYCCustomerSummaryFeedbackOutputDTO>(
                                            "dbo.GetKYCCustomerSummaryFeedback",
                                            new
                                            {
                                                BusinessProfileCode = request.BusinessProfileCode,
                                                KYCCustomerSummaryFeedbackCode = Convert.ToInt32(result.Value.Id)
                                            },
                                            UnitOfWork.Transaction, null, CommandType.StoredProcedure);

                    return Result.Success(kycCustomerSummaryFeedbackOutputDTO);
                }

                // Business Portal
                else if (ClaimCode.Business == request.CustomerSolution)
                {

                    KYCCustomerSummaryFeedback summaryFeedback = new KYCCustomerSummaryFeedback(request.KYCCustomerSummaryFeedback.KYCCustomerSummaryFeedbackCode, businessProfile, kycCategory)
                    {
                        SolutionCode = Solution.Business.Id,
                        FeedbackToTranglo = request.KYCCustomerSummaryFeedback.FeedbackToTranglo
                    };

                    var result = await _businessProfileRepo.SaveKYCCustomerSummaryFeedback(summaryFeedback);

                    if (result.IsFailure)
                    {
                        return Result.Failure<KYCCustomerSummaryFeedbackOutputDTO>(
                                    $"Save KYC Customer Summary failed for BusinessProfileCode : ${request.BusinessProfileCode}, KYC Category ID: ${kycCategory.Id} "
                                );
                    }

                    // TO-DO Work as event
                    await _businessProfileRepo.InsertKYCCustomerSummaryFeedbackNotificationAsync(new KYCCustomerSummaryFeedbackNotification
                    {
                        Event = "Created",
                        KYCCustomerSummaryFeedback = summaryFeedback,
                        BusinessProfile = businessProfile
                    }, cancellationToken);

                    KYCCustomerSummaryFeedbackOutputDTO kycCustomerSummaryFeedbackOutputDTO = await UnitOfWork.Connection.QueryFirstAsync<KYCCustomerSummaryFeedbackOutputDTO>(
                                            "dbo.GetKYCCustomerSummaryFeedback",
                                            new
                                            {
                                                BusinessProfileCode = request.BusinessProfileCode,
                                                KYCCustomerSummaryFeedbackCode = Convert.ToInt32(result.Value.Id)
                                            },
                                            UnitOfWork.Transaction, null, CommandType.StoredProcedure);

                    return Result.Success(kycCustomerSummaryFeedbackOutputDTO);
                }
                else
                {

                    return Result.Failure<KYCCustomerSummaryFeedbackOutputDTO>(
                            $"Unable to save Customer Summary Feedback for {request.BusinessProfileCode}. Check failed"
                        );

                }


            }


        }
    }
}
