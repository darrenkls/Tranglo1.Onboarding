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
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCSummary, UACAction.Create)]
    [Permission(Permission.KYCManagementReviewSummary.Action_Add_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementReviewSummary.Action_View_Code })]
    internal class SaveKYCSummaryFeedbackCommand : BaseCommand<Result<KYCSummaryFeedbackOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public KYCSummaryFeedbackInputDTO KYCSummaryFeedback { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<KYCSummaryFeedbackOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add User's KYC Details";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveKYCSummaryFeedbackCommandHandler : IRequestHandler<SaveKYCSummaryFeedbackCommand, Result<KYCSummaryFeedbackOutputDTO>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly ILogger<SaveKYCSummaryFeedbackCommandHandler> _logger;
            public IUnitOfWork UnitOfWork { get; }

            public SaveKYCSummaryFeedbackCommandHandler(
                    BusinessProfileService businessProfileService,
                    ILogger<SaveKYCSummaryFeedbackCommandHandler> logger,
                    IUnitOfWork unitOfWork
                )
            {
                _businessProfileService = businessProfileService;
                _logger = logger;
                UnitOfWork = unitOfWork;
            }

            public async Task<Result<KYCSummaryFeedbackOutputDTO>> Handle(SaveKYCSummaryFeedbackCommand request, CancellationToken cancellationToken)
            {
                var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

                var kycCategory = KYCCategory.FindById<KYCCategory>(request.KYCSummaryFeedback.KYCCategoryCode);

                if (Solution.Connect.Id == request.AdminSolution)
                {
                    KYCSummaryFeedback summaryFeedback = new KYCSummaryFeedback(request.KYCSummaryFeedback.KYCSummaryFeedbackCode, businessProfile, kycCategory)
                    {
                        SolutionCode = Solution.Connect.Id,
                        IncorrectItem = request.KYCSummaryFeedback.IncorrectItem,
                        InternalRemarks = request.KYCSummaryFeedback.InternalRemarks,
                        FeedbackToUser = request.KYCSummaryFeedback.FeedbackToUser,
                        IsResolved = false //TBT-1322 : new comment consider not resolve
                    };

                    var result = await _businessProfileService.SaveKYCSummaryFeedback(summaryFeedback);
                    var kycSummaryFeedback = result.Value;
                    if (result.IsFailure)
                    {
                        //TODO: can define failure if any issue during saving
                        return Result.Failure<KYCSummaryFeedbackOutputDTO>(
                                    $"Save KYC Summary failed for BusinessProfileCode : ${request.BusinessProfileCode}, KYC Category ID: ${kycCategory.Id} "
                                );
                    }


                    KYCSummaryFeedbackOutputDTO kycSummaryFeedbackOutputDTO = await UnitOfWork.Connection.QueryFirstAsync<KYCSummaryFeedbackOutputDTO>(
                                            "dbo.GetKYCSummaryFeedback",
                                            new
                                            {
                                                BusinessProfileCode = request.BusinessProfileCode,
                                                KYCSummaryFeedbackCode = Convert.ToInt32(result.Value.Id)
                                            },
                                            UnitOfWork.Transaction, null, CommandType.StoredProcedure);

                    return Result.Success(kycSummaryFeedbackOutputDTO);
                }

                else if (request.AdminSolution == Solution.Business.Id)
                {
                    KYCSummaryFeedback summaryFeedback = new KYCSummaryFeedback(request.KYCSummaryFeedback.KYCSummaryFeedbackCode, businessProfile, kycCategory)
                    {
                        SolutionCode = Solution.Business.Id,
                        IncorrectItem = request.KYCSummaryFeedback.IncorrectItem,
                        InternalRemarks = request.KYCSummaryFeedback.InternalRemarks,
                        FeedbackToUser = request.KYCSummaryFeedback.FeedbackToUser,
                        IsResolved = false //TBT-1322 : new comment consider not resolve
                    };

                    var result = await _businessProfileService.SaveKYCSummaryFeedback(summaryFeedback);
                    var kycSummaryFeedback = result.Value;
                    if (result.IsFailure)
                    {
                        //TODO: can define failure if any issue during saving
                        return Result.Failure<KYCSummaryFeedbackOutputDTO>(
                                    $"Save KYC Summary failed for BusinessProfileCode : ${request.BusinessProfileCode}, KYC Category ID: ${kycCategory.Id} "
                                );
                    }

                    // TO-DO Work as event
                    await _businessProfileService.InsertKYCSummaryFeedbackNotificationAsync(new KYCSummaryFeedbackNotification
                    {
                        Event = "Created",
                        KYCSummaryFeedback = summaryFeedback,
                        BusinessProfile = businessProfile
                    }, cancellationToken);

                    KYCSummaryFeedbackOutputDTO kycSummaryFeedbackOutputDTO = await UnitOfWork.Connection.QueryFirstAsync<KYCSummaryFeedbackOutputDTO>(
                                            "dbo.GetKYCSummaryFeedback",
                                            new
                                            {
                                                BusinessProfileCode = request.BusinessProfileCode,
                                                KYCSummaryFeedbackCode = Convert.ToInt32(result.Value.Id)
                                            },
                                            UnitOfWork.Transaction, null, CommandType.StoredProcedure);

                    return Result.Success(kycSummaryFeedbackOutputDTO);
                }
                else
                {

                    return Result.Failure<KYCSummaryFeedbackOutputDTO>(
                            $"Unable to save KYC Summary Feedback for {request.BusinessProfileCode}. Check failed"
                        );
                }

            }
        }
    }
}
