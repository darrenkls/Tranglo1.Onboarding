using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CommentAndReviewRemarks
{
    public class CommentAndReviewRemarksInputDTO
    {
        [Required(ErrorMessage = "Comment is required")]
        [MaxLength(1500,ErrorMessage ="Comment maximum length is 1500 characters")]
        public string Comment { get; set; }
    }
}
