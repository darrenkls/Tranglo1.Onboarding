using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCManagementInputDTO
    {
        public string PartnerNameFilter { get; set; }
        public string CountryISO2Filter { get; set; }
        public string ComplianceOfficerAssignedFilter { get; set; }
        public long WorkflowStatusCodeFilter { get; set; }
        public long KYCStatusCodeFilter { get; set; }
        public DateTime RegistrationFromFilter { get; set; }
        public DateTime RegistrationToFilter { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string SortExpression { get; set; }
        public SortDirection SortDirection { get; set; }


    }
}
