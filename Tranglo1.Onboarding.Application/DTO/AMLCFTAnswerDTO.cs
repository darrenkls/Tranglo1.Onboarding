using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTAnswerDTO
    {
        public int? AnswerChoiceCode { get; set; }
        public string AnswerChoiceDescription { get; set; }
        public int QuestionCode { get; set; }
        public bool IsAnswered { get; set; }

        [MaxLength(150, ErrorMessage = "This information must be less than 150 characters")]
        public string AnswerRemark { get; set; }

        public List<AMLCFTQuestionDTO> ChildQuestions { get; set; }
        public long? SequenceNumber { get; set; }
    }
}
