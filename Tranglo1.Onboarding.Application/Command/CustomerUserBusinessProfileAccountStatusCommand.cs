using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MediatR;
using CSharpFunctionalExtensions;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.CustomerUserList.Commands;
using Tranglo1.Onboarding.Application.Services.Identity;
using System.Linq;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.Common.Constant;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerUser, UACAction.Edit)]
    [Permission(Permission.ManageStagingUser.Action_Update_Code,
        new int[] { (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.ManageStagingUser.Action_View_Code })]
    public class CustomerUserBusinessProfileAccountStatusCommand : IRequest<Result>
    {
        public int BusinessProfileCode { get; set; }
        public string Email { get; set; }
        public UpdateCustomerUserAccountStatusInputDTO AccountStatusInput { get; set; }
    }
    public class CustomerUserBusinessProfileAccountStatusCommandHandler : IRequestHandler<CustomerUserBusinessProfileAccountStatusCommand, Result>
    {
        private readonly TrangloUserManager _userManager;
        private readonly ApplicationUserDbContext _appUserDbContext;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<UnlockUserCommandHandler> _logger;
        private readonly IBusinessProfileContext businessProfileContext;

        public CustomerUserBusinessProfileAccountStatusCommandHandler(
                ApplicationUserDbContext appUserDbContext,
                IBusinessProfileRepository businessProfileRepository,
                BusinessProfileService businessProfileService,
                TrangloUserManager userManager,
                ILogger<UnlockUserCommandHandler> logger,
                IBusinessProfileContext businessProfileContext
            )
        {
            _appUserDbContext = appUserDbContext;
            _businessProfileRepository = businessProfileRepository;
            _businessProfileService = businessProfileService;
            _userManager = userManager;
            _logger = logger;
            this.businessProfileContext = businessProfileContext;
        }
        public async Task<Result> Handle(CustomerUserBusinessProfileAccountStatusCommand request, CancellationToken cancellationToken)
        {
            var _CurrentProfile = businessProfileContext.CurrentProfileId;

            if (_CurrentProfile.HasNoValue)
            {
                //Caution: We will reach here if the caller is from Tranglo Admin
                _logger.LogError($"Unable to determine business profile. Received email: [{request.Email}]");

                return Result.Failure("Unable to determine business profile.");
            }

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.Email);
            if (applicationUser != null)
            {
                var customerUser = await _businessProfileRepository.GetCustomerUserBusinessProfileByIdAndCodeAsync(applicationUser.Id, request.BusinessProfileCode);
                //customerUser.CompanyUserAccountStatus = Enumeration.FindById<CompanyUserAccountStatus>(request.AccountStatusInput.AccountStatusCode);

                var accountStatus = await _businessProfileRepository.GetCompanyUserAccountStatus(request.AccountStatusInput?.AccountStatusCode);
                customerUser.CompanyUserAccountStatus = accountStatus;
                var result = await _businessProfileRepository.UpdateCustomerUserBusinessProfileStatusAsync(customerUser);

                if (result.IsFailure)
                {
                    return Result.Failure("Unable to update Customer User Account Status");
                } 
            }
            return Result.Success("Customer User Account Status Updated.");
        }
    }
}
