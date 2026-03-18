using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.BusinessDeclaration
{
    public class UploadBusinessDeclarationDocumentOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public long CustomerBusinessDeclarationAnswerCode { get; set; }
        public string DocumentName { get; set; }
        public Guid? DocumentId { get; set; }
    }
}
