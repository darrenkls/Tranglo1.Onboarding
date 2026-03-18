using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration
{
    public class BusinessDeclarationStatus : Enumeration
    {
		public BusinessDeclarationStatus() : base()
		{

		}
		public BusinessDeclarationStatus(int id, string name)
			: base(id, name)
		{

		}

		public static readonly BusinessDeclarationStatus Pending = new BusinessDeclarationStatus(1, "Pending");
		public static readonly BusinessDeclarationStatus Successful = new BusinessDeclarationStatus(2, "Successful");
		public static readonly BusinessDeclarationStatus Failed = new BusinessDeclarationStatus(3, "Failed");
		public static readonly BusinessDeclarationStatus Blocked = new BusinessDeclarationStatus(4, "Blocked");
	}


}