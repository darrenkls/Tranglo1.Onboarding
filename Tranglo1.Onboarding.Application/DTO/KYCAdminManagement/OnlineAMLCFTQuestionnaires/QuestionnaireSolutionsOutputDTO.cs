using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.OnlineAMLCFTQuestionnaires
{
    public class QuestionnaireSolutionsOutputDTO
    {
        public long? SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public bool isDeleted { get; set; } = false;
        public long? QuestionnaireCode { get; set; }
    }
}
