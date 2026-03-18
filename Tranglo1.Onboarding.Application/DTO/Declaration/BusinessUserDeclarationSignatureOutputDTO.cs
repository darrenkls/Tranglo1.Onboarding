using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declaration
{
    public class BusinessUserDeclarationSignatureOutputDTO
    {
        public long? BusinessProfileCode { get; set; }
        public long? BusinessUserDeclarationCode { get; set; }
        public Guid? DocumentId { get; set; }
        public string FileName { get; set; }
    }
}
