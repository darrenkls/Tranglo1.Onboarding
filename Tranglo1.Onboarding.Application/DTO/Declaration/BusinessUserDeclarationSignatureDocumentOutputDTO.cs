using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Declaration
{
    public class BusinessUserDeclarationSignatureDocumentOutputDTO
    {
        public Stream File { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}
