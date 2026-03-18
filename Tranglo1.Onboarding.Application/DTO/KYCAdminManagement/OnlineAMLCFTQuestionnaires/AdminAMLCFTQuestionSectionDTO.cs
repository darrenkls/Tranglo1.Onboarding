using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires
{
    public class AdminAMLCFTQuestionSectionDTO
    {
        public long? QuestionSectionCode { get; set; }
        public string QuestionSectionDescription { get; set; }
        public long? QuestionnaireCode { get; set; }
        public List<AdminAMLCFTQuestionDTO> Questions { get; set; }
    }
}
