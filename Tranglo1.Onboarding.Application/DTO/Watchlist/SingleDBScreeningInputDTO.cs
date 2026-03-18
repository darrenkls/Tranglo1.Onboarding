using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class SingleDBScreeningInputDTO
    {
        public string ClientReference { set; get; }
        public string Reference { set; get; }
        public string Fullname { set; get; }
        public string CompanyName { set; get; }
        public Summary Summary { set; get; }
        public bool IsChange { set; get; }
    }
}
