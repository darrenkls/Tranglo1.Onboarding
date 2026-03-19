using System;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class SignUpCodeRepository : ISignUpCodeRepository
    {
        private readonly SignUpCodeDBContext _dbContext;

        public SignUpCodeRepository(SignUpCodeDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<SignUpCode> GetSignUpCodesAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<SignUpCode> GetActiveSignUpCodeByCompanyNameAsync(string companyName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSignUpCodesAsync(SignUpCode signUpCode)
        {
            throw new NotImplementedException();
        }
    }
}
