using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.TrangloRole
{
    public class ScreenAccessMainMenuOutputDTO
    {
        
            public int MainMenuCode { get; set; }
            public string MainMenuDescription { get; set; }
            public List<ScreenAccessSubMenuOutputDTO> ScreenAccessSubMenu { get; set; }           
    }
}
