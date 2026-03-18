using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires
{
    public class AdminAMLCFTAnswerDTO
    {
        public long? AnswerChoiceCode { get; set; }
        public string AnswerChoiceDescription { get; set; }
        public long? QuestionCode { get; set; }
        public List<AdminAMLCFTQuestionDTO> ChildQuestions { get; set; }
        public long? SequenceNumber { get; set; }
    }
}
