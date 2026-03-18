using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTQuestionnaireSolutionOutputDTO
    {
        public long? SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public long? QuestionnaireCode { get; set; }
    }
}
