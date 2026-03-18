using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTQuestionDTO
    {
        public int QuestionCode { get; set; }
        public string QuestionDescription { get; set; }
        public int QuestionSectionCode { get; set; }
        public int QuestionInputTypeCode { get; set; }
        public string QuestionInputTypeDescription { get; set; }
        public int SequenceNo { get; set; }
        public int? ParentAnswerCode { get; set; }
        public int? ParentQuestionCode { get; set; }
        public bool? IsOptional { get; set; }
        public bool IsRoot { get; set; }
        public List<AMLCFTQuestionDTO> ChildQuestions { get; set; }
        public List<AMLCFTAnswerDTO> Answers { get; set; }
    }
}
