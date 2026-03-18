using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO
{
    public class UploadRegulatorLetterDocumentOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public string DocumentName { get; set; }
        public Guid? DocumentId { get; set; }
    }
}