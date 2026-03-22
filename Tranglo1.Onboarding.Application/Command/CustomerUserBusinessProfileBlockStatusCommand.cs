using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.CustomerUserList.Commands;
using Tranglo1.Onboarding.Application.Services.Identity;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.Edit)]
    [Permission(Permission.ManageStagingUser.Action_Block_Code,
        new int[] { (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    public class CustomerUserBusinessProfileBlockStatusCommand : BaseCommand<IdentityResult>
    {
        public int BusinessProfileCode { get; set; }
        public string Email { get; set; }
        public UpdateCustomerUserBlockStatusInputDTO BlockStatusInput { get; set; }
        public override Task<System.String> GetAuditLogAsync(IdentityResult result)
        {
            if (result.Succeeded)
            {
                return Task.FromResult("Blocked partner user's account");
            }

            return base.GetAuditLogAsync(result);
        }
    }
    public class CustomerUserBusinessProfileBlockStatusCommandHandler : IRequestHandler<CustomerUserBusinessProfileBlockStatusCommand, IdentityResult>
    {
        private readonly TrangloUserManager _userManager;
        private readonly ApplicationUserDbContext _appUserDbContext;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ILogger<UnlockUserCommandHandler> _logger;
        private readonly IBusinessProfileContext businessProfileContext;

        public CustomerUserBusinessProfileBlockStatusCommandHandler(
                ApplicationUserDbContext appUserDbContext,
                IBusinessProfileRepository businessProfileRepository,
                BusinessProfileService businessProfileService,
                TrangloUserManager userManager,
                ILogger<UnlockUserCommandHandler> logger,
                IHttpContextAccessor httpContextAccessor,
                IBusinessProfileContext businessProfileContext
            )
        {
            _appUserDbContext = appUserDbContext;
            _businessProfileRepository = businessProfileRepository;
            _userManager = userManager;
            _logger = logger;
            this.businessProfileContext = businessProfileContext;
        }

        public async Task<IdentityResult> Handle(CustomerUserBusinessProfileBlockStatusCommand request, CancellationToken cancellationToken)
        {
            var _CurrentProfile = businessProfileContext.CurrentProfileId;

            if (_CurrentProfile.HasNoValue)
            {
                //Caution: We will reach here if the caller is from Tranglo Admin
                _logger.LogError($"Unable to determine business profile. Received email: [{request.Email}]");

                return IdentityResult.Failed(new IdentityError()
                {
                    Description = "Unable to determine business profile."
                });
            }

            CustomerUser applicationUser = await _userManager.FindByIdAsync(request.Email) as CustomerUser;

            if (applicationUser == null)
            {
                _logger.LogError("UnlockUser", $"Email: {request.Email} is not a valid user email.");
                return IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = $"Email: {request.Email} is not a valid user email."
                        });
            }

            var customerUser = await _businessProfileRepository.GetCustomerUserBusinessProfileByIdAndCodeAsync(applicationUser.Id, request.BusinessProfileCode);
            var blockStatus = await _businessProfileRepository.GetCompanyUserBlockStatus(request.BlockStatusInput?.BlockStatusCode);
            customerUser.CompanyUserBlockStatus = blockStatus;
            await _businessProfileRepository.UpdateCustomerUserBusinessProfileStatusAsync(customerUser);
            if(customerUser.CompanyUserBlockStatus == CompanyUserBlockStatus.Block)
            {
				var lockoutEndDate = DateTimeOffset.MaxValue;
                applicationUser.SetLockoutEnd(lockoutEndDate);

				await _userManager.UpdateAsync(applicationUser);
			}
            if (customerUser.CompanyUserBlockStatus == CompanyUserBlockStatus.Unblock && applicationUser.AccountStatus == AccountStatus.Blocked)
            {
                IdentityResult ResetAccessFailedCountResponse = await _userManager.ResetAccessFailedCountAsync(applicationUser);
                if (!ResetAccessFailedCountResponse.Succeeded)
                {
                    _logger.LogError($"[CustomerUserBusinessProfileBlockStatusCommand] ResetAccessFailedCount. {ResetAccessFailedCountResponse.Errors}");
                    return ResetAccessFailedCountResponse;
                }

                IdentityResult SetLockoutEndDateResponse = await _userManager.SetLockoutEndDateAsync(applicationUser, null);
                if (!SetLockoutEndDateResponse.Succeeded)
                {
                    _logger.LogError($"[CustomerUserBusinessProfileBlockStatusCommand] SetLockoutEndDate. {SetLockoutEndDateResponse.Errors}");
                    return SetLockoutEndDateResponse;
                }
                applicationUser.SetAccountStatus(AccountStatus.Active);
                _appUserDbContext.ApplicationUsers.Update(applicationUser);
                await _appUserDbContext.SaveChangesAsync(cancellationToken);
            }
            if (customerUser.CompanyUserBlockStatus == CompanyUserBlockStatus.Unblock && applicationUser.LockoutEnd != null)
            {
				applicationUser.SetLockoutEnd(null);
				await _userManager.UpdateAsync(applicationUser);
            }

            return IdentityResult.Success;
        }
    }
}
