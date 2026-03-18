using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ParentHoldingCompany
{
    public class ParentHoldingCompanyOutputDTO
    {
        public long? ParentHoldingCompanyCode { get; set; }
        public string NameOfListedParentHoldingCompany { get; set; }
        public string CountryISO2 { get; set; }
        public string NameOfStockExchange { get; set; }
        public string StockCode { get; set; }
        public DateTime? DateOfIncorporation { get; set; }
        public bool isCompleted { get; set; }
        public Guid? ParentHoldingsConcurrencyToken { get; set; }


    }
}
