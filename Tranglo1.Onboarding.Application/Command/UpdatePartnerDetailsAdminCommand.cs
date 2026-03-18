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
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.Edit)]
    [Permission(Permission.ManagePartnerUser.Action_EditUser_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.ManagePartnerUser.Action_View_Code, Permission.ManagePartnerUser.Action_ViewDetail_Code })]
    internal class UpdatePartnerDetailsAdminCommand : BaseCommand<Result<UpdatePartnerUserAdminInputDTO>>
    {
        public string Email { get; set; }
        public UpdatePartnerUserAdminInputDTO DTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<UpdatePartnerUserAdminInputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Edited partner user details";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdatePartnerDetailsAdminCommandHandler : IRequestHandler<UpdatePartnerDetailsAdminCommand, Result<UpdatePartnerUserAdminInputDTO>>
    {
        private readonly ILogger<UpdatePartnerDetailsAdminCommandHandler> _logger;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IExternalUserRoleRepository _externalUserRoleRepository;
        private readonly TrangloUserManager _userManager;

        public UpdatePartnerDetailsAdminCommandHandler(ILogger<UpdatePartnerDetailsAdminCommandHandler> logger,
            IApplicationUserRepository applicationUserRepository,
            IBusinessProfileRepository businessProfileRepository,
            IExternalUserRoleRepository externalUserRoleRepository,
            TrangloUserManager userManager
            )
        {
            _logger = logger;
            _applicationUserRepository = applicationUserRepository;
            _businessProfileRepository = businessProfileRepository;
            _userManager = userManager;
            _externalUserRoleRepository = externalUserRoleRepository;
        }

        public async Task<Result<UpdatePartnerUserAdminInputDTO>> Handle(UpdatePartnerDetailsAdminCommand request, CancellationToken cancellationToken)
        {
            //  Notes:
            //  CUBP = CustomerUserBusinessProfile
            //  CUBPR = CustomerUserBusinessProfileRole

            var DTO = request.DTO;

            var partnerUser = await _userManager.FindByIdAsync(request.Email);

            if (partnerUser != null && partnerUser is CustomerUser user)
            {
                var name = FullName.Create(DTO.Name);
                partnerUser.SetName(name.Value);
                partnerUser.Timezone = DTO.Timezone;

                foreach (var r in DTO.CompanyRole)
                {
                    BusinessProfile businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(r.CompanyCode);
                    CustomerUser customerUser = await _userManager.FindByIdAsync(partnerUser.Email.Value) as CustomerUser;

                    CustomerUserBusinessProfile customerUserBusinessProfile = await _businessProfileRepository.GetCustomerUserBusinessProfilesByUserIdAsync(partnerUser.Id, r.CompanyCode);
                    CustomerUserBusinessProfileRole customerUserBusinessProfileRole = await _businessProfileRepository.GetCustomerUserBusinessProfileRolesByCodeAsync(customerUserBusinessProfile?.Id, r.UserRoleCode);

                    //UserRole userRole = Enumeration.FindById<UserRole>(r.UserRoleCode);
                    ExternalUserRole externalUserRole = await _externalUserRoleRepository.GetExternalRoleByRoleCodeAsync(r.UserRoleCode);

                    if (r.Action == 1)
                    {
                        if (customerUserBusinessProfile == null)
                        {
                            CustomerUserBusinessProfile newCUBP = new CustomerUserBusinessProfile(customerUser, businessProfile)
                            {
                                BusinessProfileCode = r.CompanyCode
                            };
                            
                            var resultAddCUBP = await _businessProfileRepository.AddCustomerUserBusinessProfileAsync(newCUBP);

                            if (resultAddCUBP.IsFailure)
                            {
                                return Result.Failure<UpdatePartnerUserAdminInputDTO>($"Failed to add {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving BusinessProfileCode: {r.CompanyCode}");
                            }

                            else if (customerUserBusinessProfileRole == null)
                            {
                                CustomerUserBusinessProfileRole newCUBPR = new CustomerUserBusinessProfileRole(newCUBP, externalUserRole);

                                var resultAddCUBPR = await _businessProfileRepository.AddCustomerUserBusinessProfileRoleAsync(newCUBPR);

                                if (resultAddCUBPR.IsFailure)
                                {
                                    return Result.Failure<UpdatePartnerUserAdminInputDTO>($"Failed to add {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {r.UserRoleCode}");
                                }
                            }
                        }

                        else if (customerUserBusinessProfile != null && customerUserBusinessProfileRole == null)
                        {
                            CustomerUserBusinessProfileRole newCUBPR = new CustomerUserBusinessProfileRole(customerUserBusinessProfile, externalUserRole);

                            var resultAddCUBPR = await _businessProfileRepository.AddCustomerUserBusinessProfileRoleAsync(newCUBPR);

                            if (resultAddCUBPR.IsFailure)
                            {
                                return Result.Failure<UpdatePartnerUserAdminInputDTO>($"Failed to add {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {r.UserRoleCode}");
                            }
                            
                        }
                    }

                    else if (r.Action == 2)
                    {
                        if (customerUserBusinessProfileRole != null)
                        {
                            var resultDeleteCUBPR = await _businessProfileRepository.DeleteCustomerUserBusinessProfileRoleAsync(customerUserBusinessProfileRole);

                            if (resultDeleteCUBPR.IsFailure)
                            {
                                return Result.Failure<UpdatePartnerUserAdminInputDTO>($"Failed to delete {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {r.UserRoleCode}");
                            }

                            else if (customerUserBusinessProfile != null)
                            {
                                var roles = await _businessProfileRepository.GetCustomerUserBusinessProfileRolesByCodeAsync(customerUserBusinessProfile?.Id);

                                if (roles == null)
                                {
                                    var resultDeleteCUBP = await _businessProfileRepository.DeleteCustomerUserBusinessProfileAsync(customerUserBusinessProfile);

                                    if (resultDeleteCUBP.IsFailure)
                                    {
                                        return Result.Failure<UpdatePartnerUserAdminInputDTO>($"Failed to delete {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving BusinessProfileCode: {r.CompanyCode}");
                                    }
                                }                                
                            }
                        }
                    }                    
                }

                var resultApplicationUser = await _applicationUserRepository.UpdateApplicationUser(partnerUser, cancellationToken);

                if (resultApplicationUser.IsFailure)
                {
                    return Result.Failure<UpdatePartnerUserAdminInputDTO>($"Failed to update for UserId: {partnerUser.Id}");
                }                              
            }

            List<CompanyRoleInputDTO> list = new List<CompanyRoleInputDTO>(DTO.CompanyRole);

            var outputDTO = new UpdatePartnerUserAdminInputDTO()
            {
                Name = DTO.Name,
                CompanyRole = list,
                Timezone = DTO.Timezone
            };

            return Result.Success<UpdatePartnerUserAdminInputDTO>(outputDTO);
        }
    }
}
