using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Helper.ACL
{
    public class ScreenAccessHelper
    {
        private readonly IConfiguration _config;
        private readonly AccessControlManager _accessControlManager;

        public ScreenAccessHelper(IConfiguration config, AccessControlManager accessControlManager = null)
        {
            _config = config;
            _accessControlManager = accessControlManager;
        }

        public async Task<List<ScreenAccessMenu>> GetScreenAccessList(PortalCode portalCode, string roleCode = null)
        {
            string claimListing = null;

            if (roleCode != null)
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("role", roleCode);
                claimListing = _accessControlManager.GetClaimListing(keyValuePairs);
            }

            var uacConnectionString = _config.GetConnectionString("UACConnection");

            using (var connection = new SqlConnection(uacConnectionString))
            {
                await connection.OpenAsync();
                var reader = await connection.QueryMultipleAsync(
                   "dbo.GetScreenAccessControlByClaims",
                   new
                   {
                       Claims = claimListing,
                       PortalCode = (int)portalCode
                   },
                   null, null, CommandType.StoredProcedure);

                var aclList = await reader.ReadAsync<ACLDetail>();

                var screenAccessMenuList = aclList.GroupBy(g => new { g.Menu, g.MenuOrder}).Select(o => new ScreenAccessMenu()
                {
                    MenuName = o.Key.Menu,
                    Order = o.Key.MenuOrder,
                    SubMenuList = o.GroupBy(subGroup => new { subGroup.PermissionGroupCode, subGroup.PermissionGroupName, subGroup.GroupOrder })
                                .Select(sub => new ScreenAccessMenu.SubMenuDetail()
                                {
                                    SubMenuCode = sub.Key.PermissionGroupCode,
                                    SubMenuName = sub.Key.PermissionGroupName,
                                    Order = sub.Key.GroupOrder,
                                    PermissionInfoList = sub.Select(permission =>
                                    new ScreenAccessMenu.PermissionDetail()
                                    {
                                        Order = permission.ActionOrder,
                                        ActionName = permission.UACAction,
                                        PermissionInfoCode = permission.PermissionInfoCode,
                                        RelatedPermissionInfoCodes = permission.RelatedPermissionCodes != null ?
                                            permission.RelatedPermissionCodes.Split(";").ToList() : null,
                                        IsSelected = permission.IsSelected
                                    }).OrderBy(perm => perm.Order).ToList()
                                }).OrderBy(sub => sub.Order).ToList()
                }).OrderBy(menu => menu.Order).ToList();

                return screenAccessMenuList;
            }

        }

       
        private sealed class ACLDetail
        {
            public string PermissionInfoCode { get; set; }
            public string PermissionGroupCode { get; set; }
            public string PermissionGroupName { get; set; }
            public string UACAction { get; set; }
            public string RelatedPermissionCodes { get; set; }
            public string Menu { get; set; }
            public int MenuOrder { get; set; }
            public int GroupOrder { get; set; }
            public int ActionOrder { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
