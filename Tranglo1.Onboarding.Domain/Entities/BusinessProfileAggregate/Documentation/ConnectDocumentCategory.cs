using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation
{
    public class ConnectDocumentCategory : Entity
    {
        public ConnectDocumentGroupCategory ConnectDocumentGroupCategory { get; set; }
        public DocumentCategory DocumentCategory { get; set; }
        public int GroupSequence { get; set; }
        
        private ConnectDocumentCategory()
        {
        }

        //public ConnectDocumentGroupCategory(ConnectDocumentGroupCategory connectDocumentGroupCategory, DocumentCategory documentCategory)
    }
}
