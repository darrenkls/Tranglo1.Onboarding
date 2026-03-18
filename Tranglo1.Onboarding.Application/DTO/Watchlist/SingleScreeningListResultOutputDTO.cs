using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class SingleScreeningListResultOutputDTO
    {
        public string ClientReference { set; get; }
        public string Reference { set; get; }
        public Summary Summary { set; get; }
    }
}
