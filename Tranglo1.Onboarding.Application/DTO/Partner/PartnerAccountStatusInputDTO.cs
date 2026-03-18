using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAccountStatusInputDTO
    {
        [Required(ErrorMessage ="Status is required")]
        public long Status { get; set; }
        [Required(ErrorMessage = "Change Type is required")]
        public long ChangeType { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(150,ErrorMessage = "Maximum length for description is 150 characters")]
        public string Description { get; set; }
    }
}
