using CSharpFunctionalExtensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ScreeningDetail : Entity
    {
        public ScreeningDetail()
        {
            
        }

        public long EntityId { set; get; }
        public DateTime ListingDate { set; get; }
        [ForeignKey(nameof(Screening))]
        public long? ScreeningCode { get; set; }
        [ForeignKey(nameof(ScreeningListSource))]
        public long? ScreeningListSourceCode { get; set; }
        [ForeignKey(nameof(ScreeningDetailsCategory))]
        public long? ScreeningDetailsCategoryId { get; set; }

        #region Navigation Properties
        public Screening Screening { set; get; }
        public ScreeningListSource ScreeningListSource { get; set; }
        public ScreeningDetailsCategory ScreeningDetailsCategory { set; get; }
        #endregion Navigation Properties

        /// <summary>
        /// Get ScreeningDetailsCategory (navigation property) by foreign key when it's not loaded
        /// </summary>
        /// <returns></returns>
        public ScreeningDetailsCategory GetScreeningDetailsCategory()
        {
            if (ScreeningDetailsCategoryId == null)
            {
                return null;
            }

            return Enumeration.FindById<ScreeningDetailsCategory>(ScreeningDetailsCategoryId.Value);
        }
    }
}
