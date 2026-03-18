using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTQuestionnaireOutputDTO
    {
        public int QuestionnaireCode { get; set; }
        public string QuestionnaireDescription { get; set; }
        public Guid? AMLCFTQuestionnaireConcurrencyToken { get; set; }

        public List<AMLCFTQuestionnaireSolutionOutputDTO> QuestionnaireSolutions { get; set; }
        public List<AMLCFTQuestionSectionDTO> QuestionSections { get; set; }
    }
}
