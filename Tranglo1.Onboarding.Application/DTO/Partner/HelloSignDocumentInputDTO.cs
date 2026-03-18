using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class HelloSignDocumentInputDTO
    {
        [MaxLength(150, ErrorMessage = "Hellosign document name maximum length is 150 character")]
        public string DocumentName { get; set; }
        public DateTime? SentDate { get; set; }
    }
}
