using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;
using Action = Tranglo1.UserAccessControl.UACAction;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDeclaration, UACAction.Edit)]
    [Permission(Permission.KYCManagementDeclaration.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementDeclaration.Action_View_Code })]
    internal class CreateDeclarationCommand : BaseCommand<Result<long>>
    {
        public string LoginId { get; set; }
        public int BusinessProfileCode { get; set; }
        public bool? IsNotRemittancePartner { get; set; }
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public string SigneeName { get; set; }
        public string Designation { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public override Task<string> GetAuditLogAsync(Result<long> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Declarations for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class CreateDeclarationCommandCommandHandler : IRequestHandler<CreateDeclarationCommand, Result<long>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<CreateDeclarationCommandCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IConfiguration _config;

        public CreateDeclarationCommandCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<CreateDeclarationCommandCommandHandler> logger,
                                                      TrangloUserManager userManager,
                                                      PartnerService partnerService,
                                                      IBusinessProfileRepository businessProfileRepository,
                                                      IConfiguration config)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _config = config;
        }
        public async Task<Result<long>> Handle(CreateDeclarationCommand request, CancellationToken cancellationToken)
        {
            var _BusinessProfile = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);

            if (_BusinessProfile.IsSuccess && _BusinessProfile.Value.Count>0)
            {
                var DeclarationInfo = await _businessProfileService.GetKYCDeclarationInfoAsync(request.BusinessProfileCode);
                if (DeclarationInfo != null)

                {
                    return Result.Failure<long>(
                                $"Declaration info for {request.BusinessProfileCode} already exists."
                            );
                }
                try
                {
                    var businessProfile = _BusinessProfile.Value?.FirstOrDefault();

                    

                    

                    var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Declaration.Id);

                    var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);

                    if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft || kycReviewResult != ReviewResult.Insufficient_Incomplete)
                            && (!(applicationUser is TrangloStaff) ||
                                (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                                businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted && kycReviewResult != ReviewResult.Complete))
                    {
                        return Result.Failure<long>($"Unable create declaration for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
                    }

                    var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                    var result = await _businessProfileService.InsertKYCDeclarationInfoAsync
                         (businessProfile, new Domain.Entities.Declaration(isTCRevampFeature)
                         {
                             IsDeclareTransactionTax = request.IsDeclareTransactionTax,
                             IsAgreedTermsOfService = request.IsAgreedTermsOfService,
                             IsAuthorized = request.IsAuthorized,
                             IsInformationTrue = request.IsInformationTrue,
                             IsAllApplicationAccurate = request.IsAllApplicationAccurate,
                             SigneeName =request.SigneeName,
                             Designation = request.Designation,
                             BusinessProfile = _BusinessProfile.Value.FirstOrDefault()
                         });
                    if (result != null)
                    {
                        return Result.Success<long>(result.Id);
                    }
                }
                catch(Exception ex)
                {
                    var message=string.Format("Unable to create declaration for BusinessProfileCode {0}, due to {1}", request.BusinessProfileCode,ex.Message);
                    _logger.LogError("CreateDeclarationCommand", message);
                }
            }
         
            else
            {
                _logger.LogError($"[CreateDeclarationCommand] BusinessProfileCode: {request.BusinessProfileCode} not found.");
                return Result.Failure<long>($"BusinessProfileCode: {request.BusinessProfileCode} not found.");
            }

            return Result.Failure<long>($"Unable create declaration for BusinessProfileCode: {request.BusinessProfileCode}");

        }
    }
}
