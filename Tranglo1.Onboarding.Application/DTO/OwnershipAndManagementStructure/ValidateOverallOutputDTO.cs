using System;

namespace Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure
{
    public class ValidateOverallOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public bool IsResolved { get; set; }
        public string? ShareholderDateModified { get; set; }
        public string? BoardOfDirectorDateModified { get; set; }
        public string? AuthorisedPersonDateModified { get; set; }
        public string? UBODateModified { get; set; }
        public string? DocumentationDateModified { get; set; }
        public string? DocumentationLatestModifiedToday { get; set; }
    }
}
