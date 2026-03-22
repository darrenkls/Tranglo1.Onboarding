using CSharpFunctionalExtensions;
using System;

namespace Tranglo1.Onboarding.Domain.Entities.OTP
{
    public class RequisitionOTP : Entity
    {
        public string RequisitionCode { get; set; }
        public string OTP { get; set; }
        public string RequestID { get; set; }
        public Guid? RequisitionOTPGroupId { get; set; }
    }
}
