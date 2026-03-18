using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires
{
    public class AdminAMLCFTQuestionDTO
    {
        public long? QuestionCode { get; set; }
        public string QuestionDescription { get; set; }
        public long? QuestionSectionCode { get; set; }
        public int QuestionInputTypeCode { get; set; }
        public string QuestionInputTypeDescription { get; set; }
        public int? SequenceNo { get; set; }
        public long? ParentAnswerCode { get; set; }
        public long? ParentQuestionCode { get; set; }
        public bool? IsOptional { get; set; }
        public bool IsRoot { get; set; }
        public List<AdminAMLCFTQuestionDTO> ChildQuestions { get; set; }
        public List<AdminAMLCFTAnswerDTO> Answers { get; set; }
    }
}
