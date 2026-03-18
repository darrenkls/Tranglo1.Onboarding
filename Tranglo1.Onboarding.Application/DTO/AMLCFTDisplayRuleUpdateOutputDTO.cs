using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire
{
    public class AMLCFTDisplayRuleUpdateOutputDTO
    {
        public long QuestionnaireCode { get; set; }
        public long DisplayRuleCode { get; set; }
        public long? EntityTypeCode { get; set; }
        public string EntityTypeDescription { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public string RelationshipTieUpDescription { get; set; }
        public long? ServicesOfferedCode { get; set; }
        public string ServicesOfferedDescription { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string FullName { get; set; }
    }
}
