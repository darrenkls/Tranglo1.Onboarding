using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentCategory: Entity
    {
        public string DocumentCategoryDescription { get; set; }
        public DocumentCategoryGroup DocumentCategoryGroup { get; set; }
        public int SequenceNo { get; set; }
        public bool IsActive { get; set; }
        public bool IsAMLCFT { get; set; }
        public string AdminCategoryTooltipDescription { get; set; }
        public string BusinessCategoryTooltipDescription { get; set; } // Note! use for TC and TB portal also

        public DocumentCategory()
        {

        }

        public DocumentCategory(string documentCategoryDescription, DocumentCategoryGroup documentCategoryGroup, int sequenceNo, bool isActive )
        {
            this.DocumentCategoryDescription = documentCategoryDescription;
            this.DocumentCategoryGroup = documentCategoryGroup;
            this.SequenceNo = sequenceNo;
            this.IsActive = isActive;
        }
    }
}
