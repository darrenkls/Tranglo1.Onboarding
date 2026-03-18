using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class APIAccessWhitelistIPsOutputDTO
    {
        public string IPAddressStart { get; set; }
        public string IPAddressEnd { get; set; }
        public bool IsRangeIPAddress { get; set; }
    }
}
