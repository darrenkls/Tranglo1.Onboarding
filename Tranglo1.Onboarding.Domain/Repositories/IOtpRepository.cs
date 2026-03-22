using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.OTP;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IOtpRepository
    {
        Task NewRequisitionOTPAsync(RequisitionOTP requisitionOTP);
        Task<bool> ValidateOTPAsync(RequisitionOTP requisitionOTP, int userId);
    }
}
