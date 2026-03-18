using System;
using Tranglo1.Onboarding.Domain.Common.Extensions;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Requests
{
    public class NameScreenerRequest
    {
        public string EntityType { get; private set; }
        public string FullName { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string Nationality { get; private set; }
        public string Gender { get; private set; }

        public NameScreenerRequest(ScreeningEntityType entityType, string fullName, DateTime? dateOfBirth, 
            string nationality, string gender)
        {
            EntityType = entityType?.Name;
            FullName = fullName;
            Nationality = nationality;
            Gender = gender;

            PatchDateOfBirth();
        }

        #region Private Helper Methods
        /// <summary>
        /// DateOfBirth only accepts DateTime with Kind = Unspecified; other kinds cause ScreeningAPI errors.
        /// Example: "1953-07-23T00:00:00" is valid; "1953-07-23T00:00:00Z" is invalid.
        /// </summary>
        private void PatchDateOfBirth()
        {
            if (!DateOfBirth.HasValue)
            {
                return;
            }

            // db datetime is in UTC but Screening API expects +8 timezone without timezone info in the datetime
            // If don't do so, it might impact the accurancy of the screening result.
            var dob = DateOfBirth.UTCToMalaysiaTime();

            DateOfBirth = DateTime.SpecifyKind(dob.Date, DateTimeKind.Unspecified);
        } 
        #endregion Private Helper Methods
    }
}
