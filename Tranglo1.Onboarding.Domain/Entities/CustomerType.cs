using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerType : Enumeration
    {
        public string DescriptionExternal { get; set; }
        public int CustomerTypeGroupCode { get; set; }

        public CustomerType() : base() { }

        public CustomerType(int id, string name, string descriptionExternal, int customerTypeGroupCode)
            : base(id, name)
        {
            DescriptionExternal = descriptionExternal;
            CustomerTypeGroupCode = customerTypeGroupCode;
        }

        public static readonly CustomerType Individual = new CustomerType(1, "Individual", "Individual", 1);
        public static readonly CustomerType Corporate_Normal_Corporate = new CustomerType(2, "Normal corporate payout", "Corporate", 2);
        public static readonly CustomerType Corporate_Cryptocurrency_Exchange = new CustomerType(3, "Cryptocurrency exchange", "Cryptocurrency", 3);
        public static readonly CustomerType Remittance_Partner = new CustomerType(4, "Remittance partner", "Money Service Business", 4);
        public static readonly CustomerType Corporate_Mass_Payout = new CustomerType(5, "Mass payout", "Corporate", 2);
    }

    public static class CustomerTypeExtensions
    {
        public static bool IsIndividual(this CustomerType customerType)
        {
            return customerType == CustomerType.Individual;
        }

        public static bool IsCorporate(this CustomerType customerType)
        {
            return customerType == CustomerType.Corporate_Normal_Corporate || customerType == CustomerType.Corporate_Mass_Payout;
        }

        public static bool IsCryptocurrency(this CustomerType customerType)
        {
            return customerType == CustomerType.Corporate_Cryptocurrency_Exchange;
        }

        public static bool IsRemittancePartner(this CustomerType customerType)
        {
            return customerType == CustomerType.Remittance_Partner;
        }
    }
}
