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
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.Edit)]
    [Permission(Permission.ManageStagingUser.Action_Update_Code,
        new int[] { (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.ManageStagingUser.Action_View_Code })]
    internal class UpdatePartnerDetailsCustomerCommand : BaseCommand<Result<UpdatePartnerUserCustomerInputDTO>>
    {
        public string Email { get; set; }
        public int BusinessProfileCode { get; set; }
        public UpdatePartnerUserCustomerInputDTO DTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<UpdatePartnerUserCustomerInputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Customer Partner User Details for Email: [{Email}] and BusinessProfileCode: [{BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdatePartnerDetailsCustomerCommandHandler : IRequestHandler<UpdatePartnerDetailsCustomerCommand, Result<UpdatePartnerUserCustomerInputDTO>>
    {
        private readonly ILogger<UpdatePartnerDetailsCustomerCommandHandler> _logger;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IExternalUserRoleRepository _externalUserRoleRepository;
        private readonly TrangloUserManager _userManager;

        public UpdatePartnerDetailsCustomerCommandHandler(ILogger<UpdatePartnerDetailsCustomerCommandHandler> logger,
            IApplicationUserRepository applicationUserRepository,
            IBusinessProfileRepository businessProfileRepository,
            IExternalUserRoleRepository externalUserRoleRepository,
            TrangloUserManager userManager
            )
        {
            _logger = logger;
            _applicationUserRepository = applicationUserRepository;
            _businessProfileRepository = businessProfileRepository;
            _externalUserRoleRepository = externalUserRoleRepository;
            _userManager = userManager;
        }

        public async Task<Result<UpdatePartnerUserCustomerInputDTO>> Handle(UpdatePartnerDetailsCustomerCommand request, CancellationToken cancellationToken)
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

                foreach (var r in DTO.UserRole)
                {
                    BusinessProfile businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
                    CustomerUser customerUser = await _userManager.FindByIdAsync(partnerUser.Email.Value) as CustomerUser;

                    CustomerUserBusinessProfile customerUserBusinessProfile = await _businessProfileRepository.GetCustomerUserBusinessProfilesByUserIdAsync(partnerUser.Id, request.BusinessProfileCode);
                    CustomerUserBusinessProfileRole customerUserBusinessProfileRole = await _businessProfileRepository.GetCustomerUserBusinessProfileRolesByCodeAsync(customerUserBusinessProfile?.Id, r.UserRoleCode);

                    //UserRole userRole = Enumeration.FindById<UserRole>(r.UserRoleCode);
                    ExternalUserRole externalUserRole = await _externalUserRoleRepository.GetExternalRoleByRoleCodeAsync(r.UserRoleCode);
                    
                    if (r.Action == 1)
                    {                      
                        if (customerUserBusinessProfileRole == null)
                        {
                            CustomerUserBusinessProfileRole newCUBPR = new CustomerUserBusinessProfileRole(customerUserBusinessProfile, externalUserRole);

                            var resultAddCUBPR = await _businessProfileRepository.AddCustomerUserBusinessProfileRoleAsync(newCUBPR);

                            if (resultAddCUBPR.IsFailure)
                            {
                                return Result.Failure<UpdatePartnerUserCustomerInputDTO>($"Failed to add {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {r.UserRoleCode}");
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
                                return Result.Failure<UpdatePartnerUserCustomerInputDTO>($"Failed to delete {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {r.UserRoleCode}");
                            }                            
                        }
                    }                    
                }

                var resultApplicationUser = await _applicationUserRepository.UpdateApplicationUser(partnerUser, cancellationToken);

                if (resultApplicationUser.IsFailure)
                {
                    return Result.Failure<UpdatePartnerUserCustomerInputDTO>($"Failed to update for UserId: {partnerUser.Id}");
                }                              
            }

            List<UserRolesInputDTO> list = new List<UserRolesInputDTO>(DTO.UserRole);

            var outputDTO = new UpdatePartnerUserCustomerInputDTO()
            {
                Name = DTO.Name,
                UserRole = list,
                Timezone = DTO.Timezone
            };

            return Result.Success<UpdatePartnerUserCustomerInputDTO>(outputDTO);
        }
    }
}
