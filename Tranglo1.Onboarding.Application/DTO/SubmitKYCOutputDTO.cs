using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO
{
    public class SubmitKYCOutputDTO
    {
        public DateTime? ReviewConcurrentLastModified { get; set; }
        public Guid? ReviewConcurrencyToken { get; set; }
    }
}
