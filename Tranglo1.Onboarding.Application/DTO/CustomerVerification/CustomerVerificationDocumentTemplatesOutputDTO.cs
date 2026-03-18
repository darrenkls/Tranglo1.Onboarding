using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class CustomerVerificationDocumentTemplatesOutputDTO
    {
        public long? CustomerVerificationCode { get; set; }
        public Guid? TemplateID { get; set; }
    }
}
