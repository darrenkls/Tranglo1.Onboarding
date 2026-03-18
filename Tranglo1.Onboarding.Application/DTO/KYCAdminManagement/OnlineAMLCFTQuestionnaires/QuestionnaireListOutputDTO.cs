using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires
{
    public class QuestionnaireListOutputDTO
    {
        public long QuestionnaireCode { get; set; }
        public string QuestionnaireDescription { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public List<QuestionnaireSolutionList> QuestionnaireSolutionList { get; set; }
    }

    public class QuestionnaireSolutionList
    {
        public long QuestionnaireCode { get; set; }
        public long? SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
    }
}
