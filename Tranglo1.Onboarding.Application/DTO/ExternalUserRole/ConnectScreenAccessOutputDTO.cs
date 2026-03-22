using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.IdentityServer.Helper.ACL;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class ConnectScreenAccessOutputDTO
    {
        public List<ScreenAccessMenu> ScreenAccessMenuList { get; set; }
    }
}
