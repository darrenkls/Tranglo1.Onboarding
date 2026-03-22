using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation
{
    public class BusinessDocumentGroupCategory : Entity
    {
        public string GroupCategoryDescription { get; set; }
        public string TooltipDescription { get; set; }

        private BusinessDocumentGroupCategory()
        {
        }
    }
}
