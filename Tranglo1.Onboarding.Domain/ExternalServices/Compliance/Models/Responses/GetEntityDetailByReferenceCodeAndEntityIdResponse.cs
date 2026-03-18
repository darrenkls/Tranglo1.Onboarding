using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses
{
    public class GetEntityDetailByReferenceCodeAndEntityIdResponse
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PrimaryName { get; set; }
        public string Nationality { get; set; }
        public string DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Country { get; set; }
        public string Gender { get; set; }
        public string EntryType { get; set; }
        public string EntryCategory { get; set; }
        public string EntrySubCategory { get; set; }
        public string Positions { get; set; }
        public DateTime? ListDate { get; set; }
        public int? ListSource { get; set; }
        public string ListSourceName
        {
            get
            {
                if (!ListSource.HasValue)
                {
                    return string.Empty;
                }

                var listSource = Enumeration.FindById<ScreeningListSource>(ListSource.Value);
                return listSource?.Name ?? string.Empty;
            }
        }
        public long? EntityId { get; set; }
        public bool? HasNegativeNews { get; set; }
        public string Comments { get; set; }
        public string NameSource { get; set; }
        public List<string> SourceWebLinks { get; set; }
    }
}