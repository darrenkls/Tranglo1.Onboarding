using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentCategoryGroup: Entity
    {
        public string DocumentCategoryGroupDescription { get; set; }
        public Solution Solution { get; set; }
        public int CustomerTypeGroupCode { get; set; }

        public DocumentCategoryGroup()
        {

        }

        public DocumentCategoryGroup(string documentCategoryGroupDescription, Solution solution)
        {
            this.DocumentCategoryGroupDescription = documentCategoryGroupDescription;
            this.Solution = solution;
        }
    }
}
