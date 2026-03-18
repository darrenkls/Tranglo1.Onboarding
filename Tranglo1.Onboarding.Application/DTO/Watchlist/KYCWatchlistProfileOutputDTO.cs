using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCWatchlistProfileOutputDTO
    {
        public string Partner { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public DateTime DateOfBirthOrIncorporation { get; set; }
        public string Gender { get; set; }
        public string IDNumber { get; set; }
        public int BusinessProfileId { get; set; }
        public int OwnershipStructureTypeId { get; set; }
        public int TableId { get; set; }
        public long? EnforcementActionsCode { get; set; }
        public string EnforcementActionsDescription { get; set; }
    }
}
