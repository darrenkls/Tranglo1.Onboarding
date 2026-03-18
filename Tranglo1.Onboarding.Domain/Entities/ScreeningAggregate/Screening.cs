using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses;

namespace Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate
{
    public class Screening : Entity
    {
        public Screening()
        {

        }

        public Guid ReferenceCode { set; get; }
        public int PEPCount { set; get; }
        public int SanctionCount { set; get; }
        public int SOECount { set; get; }
        public int AdverseMediaCount { set; get; }
        public int EnforcementCount { get; set; }
        public int AssociatedEntityCount { get; set; }
        public DateTime ScreeningDate { set; get; }

        [ForeignKey(nameof(ScreeningInput))]
        public int? ScreeningInputCode { get; set; }

        #region Navigation Properties
        public ScreeningInput ScreeningInput { get; set; }
        public ICollection<ScreeningDetail> ScreeningDetails { get; set; } = new List<ScreeningDetail>();
        #endregion Navigation Properties

        public void AddScreeningDetails(IEnumerable<GetEntityDetailByReferenceCodeResponse> getMatchedEntityResponses)
        {
            foreach (var getMatchedEntityResponse in getMatchedEntityResponses)
            {
                ScreeningDetails.Add(new ScreeningDetail()
                {
                    EntityId = getMatchedEntityResponse.EntityId,
                    ListingDate = getMatchedEntityResponse.ListDate.HasValue ? getMatchedEntityResponse.ListDate.Value : DateTime.MinValue,
                    ScreeningListSourceCode = getMatchedEntityResponse.ListSource,
                    ScreeningDetailsCategoryId = getMatchedEntityResponse.GetScreeningDetailCategory()?.Id
                });
            }
        }
    }
}
