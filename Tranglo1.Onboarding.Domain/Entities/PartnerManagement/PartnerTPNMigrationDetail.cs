using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class PartnerTPNMigrationDetail : Entity
    {
		public long TPNPartnerCode { get; set; }
		public long PartnerCode { get; set; }
		public int BusinessProfileCode { get; set; }
		public int? SessionId { get; set; }
		public DateTime? SessionDate { get; set; }
		public bool? isDocUploaded { get; set; }
		public bool? isAgreementUploaded { get; set; }
	}
}