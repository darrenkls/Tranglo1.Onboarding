using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.DTO.Documentation.AdminTemplateManagementInputDTO
{
    public class AdminTemplateManagementInputDTO
    {
        public long? QuestionnaireCode { get; set; }
        public long SolutionCode { get; set; }
        public List<TrangloEntity> TrangloEntities { get; set; }
        

    }

    public class TrangloEntity
    {
       public  long TrangloEntityCode { get; set; }
       public bool? IsChecked { get; set; }
    }
}
