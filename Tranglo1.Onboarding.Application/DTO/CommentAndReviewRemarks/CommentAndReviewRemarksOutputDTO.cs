using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.CommentAndReviewRemarks
{
    public class CommentAndReviewRemarksOutputDTO
    {
        public string username { get; set; }
        public DateTime dateCreated { get; set; }
        public string comment { get; set; }

        public Guid DocumentId { get; set; }
        public long DocumentCommentBPCode { get; set; }
        public string FileName { get; set; }


    }
}
