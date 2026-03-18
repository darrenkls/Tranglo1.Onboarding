using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
	public class ChangeType : Enumeration
	{
		public ChangeType() : base()
		{

		}

		public ChangeType(int id, string name)
			: base(id, name)
		{

		}


		public static readonly ChangeType KYC = new ChangeType(1, "KYC");
		public static readonly ChangeType COMMERCIAL = new ChangeType(2, "COMMERCIAL");
		public static readonly ChangeType TECH = new ChangeType(3, "TECH");
		public static readonly ChangeType TEST_SIGN_UP = new ChangeType(4, "TEST SIGN UP");


	}
}
