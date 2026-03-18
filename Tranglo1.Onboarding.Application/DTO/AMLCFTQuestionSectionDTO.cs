using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTQuestionSectionDTO
    {
        public int QuestionSectionCode { get; set; }
        public string QuestionSectionDescription { get; set; }
        public int QuestionnaireCode { get; set; }
        public List<AMLCFTQuestionDTO> Questions { get; set; }
    }
}
