using System;
using System.Collections.Generic;
using System.Text;

namespace CMSIntegrationTask.DTO
{
	public class GetWalletRequisitionStatusInputDto
	{
		public class TransactionDetails
		{
			public string TrnxID { get; set; }
			public string RequisitionCode { get; set; }
			public long T1_Partner_Key { get; set; }
		}
		public List<TransactionDetails> Transactions { get; set; }

	}
}
