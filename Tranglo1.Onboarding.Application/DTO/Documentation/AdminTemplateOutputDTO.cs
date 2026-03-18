using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Documentation.AdminTemplateOutputDTO
{
    public class AdminTemplateOutputDTO
    {
        public int CategoryId { get; set; }
        public string CategoryDescription { get; set; }
        public string TemplateTypeDescription { get; set; }
        public Guid DocumentId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }  
        public long? QuestionnaireCode { get; set; }
        public bool IsActive { get; set; }
        public int SolutionCode { get; set; }
        public List<TrangloEntity> TrangloEntities {  get; set; }
    }

    public class TrangloEntity
    {
        public long TrangloEntityCode { get; set; }
        public int CategoryId { get; set; }
        public long? QuestionnaireCode { get; set; }
        public bool isChecked { get; set; }
    }
}
