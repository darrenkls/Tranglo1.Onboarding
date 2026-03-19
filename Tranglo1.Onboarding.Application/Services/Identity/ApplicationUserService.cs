using System;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Services.Identity
{
    public class ApplicationUserService
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IPartnerRepository _partnerRepository;

        public ApplicationUserService(
            IApplicationUserRepository applicationUserRepository,
            IPartnerRepository partnerRepository)
        {
            _applicationUserRepository = applicationUserRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task<bool> UserHasTrangloEntity(TrangloStaff trangloStaff, string trangloEntityCode)
        {
            throw new NotImplementedException();
        }
    }
}
