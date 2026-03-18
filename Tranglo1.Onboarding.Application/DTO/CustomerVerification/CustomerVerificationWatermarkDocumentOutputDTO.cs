using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CustomerVerification
{
    public class CustomerVerificationWatermarkDocumentOutputDTO
    {

        public long? CustomerVerificationDocumentCode { get; set; }
        public Guid? WatermarkDocumentID { get; set; }
        public string WatermarkDocumentName { get; set; }
        public long? VerificationIDTypeSectionCode { get; set; }
        public string VerificationIDTypeSectionDescription { get; set; }
        public long? VerificationIDType { get; set; }
        public string VerificationIDTypeDescription { get; set; }
    }
}
