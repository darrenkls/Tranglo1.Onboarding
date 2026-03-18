using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires
{
    public class AdminAMLCFTQuestionnaireOutputDTO
    {
        public long? QuestionnaireCode { get; set; }
        public string QuestionnaireDescription { get; set; }
        public List<QuestionnaireSolutionsOutputDTO> QuestionnaireSolutions { get; set;}
        public List<AdminAMLCFTQuestionSectionDTO> QuestionSections { get; set; }
    }

  /*  public class QuestionnaireSolutions
    {
        public long? SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public bool isDeleted { get; set; } = false;
        public long? QuestionnaireCode { get; set; }

    }*/
}
