using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTDisplayRuleUpdateInputDTO
    {
        public long? RelationshipTieUpCode { get; set; }
        public long? EntityTypeCode { get; set; }
        public long? ServicesOfferedCode { get; set; }
        public long QuestionnaireCode { get; set; }
        public int? DisplayRuleCode { get; set; }
        public int ActionCode { get; set; }
    }
}
