using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ComplianceRequisitionType : Enumeration
    {
        public long ComplianceSettingTypeCode { get; set; }

        public ComplianceRequisitionType() : base() { }

        public ComplianceRequisitionType(int id, long complianceSettingTypeCode, string name)
            : base(id, name)
        {
            ComplianceSettingTypeCode = complianceSettingTypeCode;
        }

        public static readonly ComplianceRequisitionType Add_Sender_Compliance_Limit_Setting = new ComplianceRequisitionType(1, ComplianceSettingType.Sender_Compliance_Limit_Setting.Id, "Add New Sender Compliance Limit");
        public static readonly ComplianceRequisitionType Edit_Sender_Compliance_Limit_Setting = new ComplianceRequisitionType(2, ComplianceSettingType.Sender_Compliance_Limit_Setting.Id, "Edit Sender Compliance Limit");
        public static readonly ComplianceRequisitionType Update_Compliance_Internal_Risk_Rating = new ComplianceRequisitionType(3, ComplianceSettingType.RBA.Id, "Update Compliance Internal Risk Rating");
    }
}
