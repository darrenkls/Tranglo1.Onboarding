using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAgreementTemplateOutputDTO
    {
        public long PartnerCode { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string RemovedBy { get; set; }
        public DateTime? RemovedAt { get; set; }
        public bool IsRemoved { get; set; }
    }
}