using System.Collections.Generic;
using System.Linq;
using Tranglo1.Onboarding.Domain.Common;


namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangleDepartment : Enumeration
    {
        public string DepartmentPrefix { get; set; }

        public TrangleDepartment() : base() { }

        public TrangleDepartment(int id, string name, string departmentPrefix)
            : base(id, name)
        {
            DepartmentPrefix = departmentPrefix;
        }

        public static IEnumerable<TrangleDepartment> GetAllTrangleDepartment()
        {
            return GetAll<TrangleDepartment>();
        }

        public static TrangleDepartment GetDepartmentByPrefix(string deptPrefix)
        {
            var deptPrefixValue = deptPrefix?.ToUpper();
            return GetAllTrangleDepartment().FirstOrDefault(x => x.DepartmentPrefix == deptPrefixValue);
        }

        public static readonly TrangleDepartment Finance = new TrangleDepartment(1, "Finance", "FIN");
        public static readonly TrangleDepartment SalesOperation = new TrangleDepartment(2, "Sales Operation", "SO");
        public static readonly TrangleDepartment RevenueAssurance = new TrangleDepartment(3, "Revenue Assurance", "RA");
        public static readonly TrangleDepartment Treasury = new TrangleDepartment(4, "Treasury", "TRE");
        public static readonly TrangleDepartment Product = new TrangleDepartment(5, "Product", "PROD");
        public static readonly TrangleDepartment Compliance = new TrangleDepartment(6, "Compliance", "COMP");
        public static readonly TrangleDepartment Technology = new TrangleDepartment(7, "Technology", "TECH");
        public static readonly TrangleDepartment CustomerSupport = new TrangleDepartment(8, "Customer Support", "CUST");
        public static readonly TrangleDepartment Management = new TrangleDepartment(9, "Management", "M");
        public static readonly TrangleDepartment BusinessDevelopment = new TrangleDepartment(10, "Business Development", "BD");
        public static readonly TrangleDepartment DigitalMarketing = new TrangleDepartment(11, "Digital Marketing", "DIGM");
        public static readonly TrangleDepartment ExecutiveSecretary = new TrangleDepartment(12, "Executive Secretary", "SEC");
        public static readonly TrangleDepartment HR = new TrangleDepartment(13, "HR", "HR");
        public static readonly TrangleDepartment Infrastructure = new TrangleDepartment(14, "Infrastructure", "INFR");
        public static readonly TrangleDepartment Legal = new TrangleDepartment(15, "Legal", "LEG");
        public static readonly TrangleDepartment ProcessExcellence = new TrangleDepartment(16, "Process Excellence", "PE");
        public static readonly TrangleDepartment InternalAudit = new TrangleDepartment(17, "Internal Audit", "INTA");
    }
}
