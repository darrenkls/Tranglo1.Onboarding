using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentCategoryBP: Entity
    {
        private BusinessProfile businessProfile;
        private DocumentCategory documentCategoryInfo;

        public DocumentCategory DocumentCategory { get; set; }
        public BusinessProfile BusinessProfile { get; set; }
        public long DocumentCategoryBPStatusCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public long DocumentCategoryCode { get; set; }
        public DocumentCategoryBPStatus DocumentCategoryBPStatus { get; set; }

        public DocumentCategoryBP()
        {

        }

        public DocumentCategoryBP(BusinessProfile businessProfile, DocumentCategory documentCategoryInfo)
        {
            this.businessProfile = businessProfile;
            this.BusinessProfileCode = businessProfile.Id;
            this.documentCategoryInfo = documentCategoryInfo;
            this.DocumentCategoryCode = documentCategoryInfo.Id;
            this.DocumentCategoryBPStatusCode = 1;
        }
    }
}
