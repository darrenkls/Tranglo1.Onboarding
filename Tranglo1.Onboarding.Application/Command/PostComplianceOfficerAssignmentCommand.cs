using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCCOInformation, UACAction.Edit)]
    internal class PostComplianceOfficerAssignmentCommand : BaseCommand<Result<KYCComplianceOfficerOutputDTO>>
    {
        public string COOfficerAssignedLoginID { set; get; }
        public int BusinessProfileCode { get; set; }
        public string LoginId { get; set; }
        public int AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<KYCComplianceOfficerOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Assigned Compliance Officer to KYC case";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class PostComplianceOfficerAssignmentCommandHandler : IRequestHandler<PostComplianceOfficerAssignmentCommand, Result<KYCComplianceOfficerOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly IApplicationUserRepository _applicationUserRepository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly ILogger<PostComplianceOfficerAssignmentCommand> _logger;
            private readonly PartnerService _partnerService;
            private readonly TrangloUserManager _userManager;
            public PostComplianceOfficerAssignmentCommandHandler(
                    IMapper mapper,
                    IApplicationUserRepository applicationUserRepository,
                    BusinessProfileService businessProfileService,
                    ILogger<PostComplianceOfficerAssignmentCommand> logger,
                    PartnerService partnerService,
                    TrangloUserManager userManager
                )
            {
                _applicationUserRepository = applicationUserRepository;
                _businessProfileService = businessProfileService;
                _logger = logger;
                _mapper = mapper;
                _partnerService = partnerService;
                _userManager = userManager;
            }

            public async Task<Result<KYCComplianceOfficerOutputDTO>> Handle(PostComplianceOfficerAssignmentCommand request, CancellationToken cancellationToken)
            {

                var ApplicationUser = await _applicationUserRepository.GetApplicationUserByLoginId(request.COOfficerAssignedLoginID);
                if (ApplicationUser == null)
                {
                    return Result.Failure<KYCComplianceOfficerOutputDTO>(
                            $"Application User {request.COOfficerAssignedLoginID} Not Exist."
                        );
                }
                var complianceOfficerName = ApplicationUser.LoginId;
                var businessProfile = _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode).Result.Value;
                WorkflowStatus workflowStatus = new WorkflowStatus();
                WorkflowStatus businessKYCWorkflowStatus = new WorkflowStatus();
                CollectionTier businessCollectionTier = null;
                
                if (request.AdminSolution == Solution.Connect.Id)
                {
                    businessCollectionTier = null;
                    if (businessProfile.WorkflowStatus == null)
                    {
                        workflowStatus = null;
                    }
                    else
                    {
                        workflowStatus = Enumeration.FindById<WorkflowStatus>(businessProfile.WorkflowStatus.Id);
                    }

                    businessProfile.SetComplianceOfficer(request.COOfficerAssignedLoginID, workflowStatus);
                }
                else if (request.AdminSolution == Solution.Business.Id)
                {
                    businessCollectionTier = Enumeration.FindById<CollectionTier>(businessProfile.CollectionTier.Id);

                    if (businessProfile.BusinessWorkflowStatus == null)
                    {
                        businessKYCWorkflowStatus = null;
                    }
                    else
                    {
                        businessKYCWorkflowStatus = Enumeration.FindById<WorkflowStatus>(businessProfile.BusinessWorkflowStatus.Id);
                    }

                    businessProfile.SetBusinessComplianceOfficer(request.COOfficerAssignedLoginID, businessCollectionTier, businessKYCWorkflowStatus);

                }
                
                
                await _businessProfileService.UpdateBusinessProfileAsync(businessProfile);
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

                var result = new KYCComplianceOfficerOutputDTO()
                {
                    ComplianceOfficerAssignedLoginId = request.COOfficerAssignedLoginID,
                    ComplianceOfficerAssignedName = complianceOfficerName
                };
                return Result.Success(result);
            }
        }
    }

}
