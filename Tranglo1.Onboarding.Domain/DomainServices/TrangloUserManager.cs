using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Domain.DomainServices
{
    public class TrangloUserManager : UserManager<ApplicationUser>
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly BusinessProfileService _businessProfileService;

        public TrangloUserManager(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger,
            IApplicationUserRepository applicationUserRepository,
            BusinessProfileService businessProfileService)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _applicationUserRepository = applicationUserRepository;
            _businessProfileService = businessProfileService;
        }

        public override async Task<ApplicationUser> FindByIdAsync(string loginId)
        {
            return await _applicationUserRepository.GetApplicationUserByLoginId(loginId);
        }

        public async Task<List<PartnerSubscription>> GetPartnerSubscriptionsForUserAsync(CustomerUser customerUser)
        {
            return await _businessProfileService.GetPartnerSubscriptionByUserIdAsync((int)customerUser.Id);
        }
    }
}
