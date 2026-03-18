using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation
{
    public class InternalDocumentUpload : Entity
    {
        public BusinessProfile BusinessProfile;
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public bool IsRemoved { get; set; } = false;
        public int? RemovedBy { get; set; }
        public DateTime? RemovedDate { get; set; }
        public bool IsDisplay { get; set; } = true;


    }
}
