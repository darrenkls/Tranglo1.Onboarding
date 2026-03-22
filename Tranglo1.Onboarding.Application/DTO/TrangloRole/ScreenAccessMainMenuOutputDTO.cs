using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.IdentityServer.DTO.TrangloRole;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class ScreenAccessMainMenuOutputDTO
    {
        
            public int MainMenuCode { get; set; }
            public string MainMenuDescription { get; set; }
            public List<ScreenAccessSubMenuOutputDTO> ScreenAccessSubMenu { get; set; }           
    }
}
