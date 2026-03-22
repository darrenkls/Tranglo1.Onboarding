using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Tranglo1.Onboarding.Domain.Common.Extensions;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.Common.SingleScreening
{
    public class ChangeDTO
    {
        [Name("Solution")]
        public string SolutionNames { get; set; }
        [Name("Partner Name")]
        public string CompanyName { set; get; }
        [Name("Category")]
        public string OwnershipStructureTypeNames { get; set; }
        [Name("Type")]
        public string EntityTypeName { get; set; }
        [Name("Name")]
        public string FullName { set; get; }
        [Name("Nationality")]
        public string NationalityFullName { get; set; }
        [Name("Date of Birth / Date of Incorporation")]
        public string DateOfBirth { get; set; }
        [Name("Type of Alert")]
        public string ScreeningDetailCategoryNames { set; get; }
        [Name("Entity ID")]
        public string EntityIds { get; set; }

        public ChangeDTO()
        {
        }

        public ChangeDTO(List<Solution> solutions, string companyName, OwnershipStrucureType ownershipStrucureType,
            ScreeningEntityType screeningEntityType, string fullName, CountryMeta nationality,
            DateTime? dateOfBirth, IEnumerable<string> screeningDetailCategories, IEnumerable<long> entityIds)
        {
            var solutionShortNames = solutions
                .Select(x => x?.GetShortName())
                .OrderBy(x => x)
                .Distinct();
            SolutionNames = string.Join(", ", solutionShortNames);

            CompanyName = companyName;
            OwnershipStructureTypeNames = ownershipStrucureType?.Name;
            EntityTypeName = screeningEntityType?.ExternalDescription;
            FullName = fullName;
            NationalityFullName = nationality?.Name;
            DateOfBirth = dateOfBirth.UTCToMalaysiaTime().ToString("dd/MM/yyyy");
            ScreeningDetailCategoryNames = string.Join(", ", screeningDetailCategories.Where(x => !string.IsNullOrEmpty(x)));
            EntityIds = string.Join(", ", entityIds);
        }
    }
}
