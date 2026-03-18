using System;
using System.Collections.Generic;
using System.Text;

namespace CMSIntegrationTask.DTO
{
	public class GetWalletRequisitionStatusOutputDto
	{
		public class TransactionDetails
		{
			public string Status { get; set; }
			public string TrnxID { get; set; }
			public string RequisitionCode { get; set; }
			public int? T1_Partner_Key { get; set; }
		}
		public List<TransactionDetails> Result { get; set; }
		public bool OperationStatus { get; set; }
		public string ResponseMessage { get; set; }
	}
}
