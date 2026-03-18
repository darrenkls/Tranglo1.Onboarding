using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Documentation
{
    public class DocumentMetaDataOutputDTO
    {
        public Guid documentId { get; set; }
        public string fileName { get; set; }
        public DateTime dateCreated { get; set; }
        public bool IsVerified { get; set; }
        public DateTime VerifiedDate { get; set; }

    }
}
