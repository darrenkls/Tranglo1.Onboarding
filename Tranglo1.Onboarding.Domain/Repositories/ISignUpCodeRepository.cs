using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface ISignUpCodeRepository
    {
        Task<SignUpCode> GetSignUpCodesAsync(string code);
        Task<SignUpCode> GetActiveSignUpCodeByCompanyNameAsync(string companyName);
        Task UpdateSignUpCodesAsync(SignUpCode signUpCode);
    }
}
