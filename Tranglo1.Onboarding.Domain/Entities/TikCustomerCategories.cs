using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TikCustomerCategories : Enumeration
    {
        public long CustomerTypeCode { get; set; }

        public TikCustomerCategories() : base() { }

        public TikCustomerCategories(int customerCategoryCode, string description, int customerTypeCode)
            : base(customerCategoryCode, description)
        {
            CustomerTypeCode = customerTypeCode;
        }

        public static readonly TikCustomerCategories Crypto_Currency_Exchange = new TikCustomerCategories(1, "Crypto Currency Exchange", 3);
        public static readonly TikCustomerCategories Mass_Payout = new TikCustomerCategories(2, "Mass Payout", 5);
        public static readonly TikCustomerCategories Normal_Corporate = new TikCustomerCategories(3, "Normal Corporate", 2);
        public static readonly TikCustomerCategories Remittance_Partner = new TikCustomerCategories(4, "Remittance Partner", 4);
    }
}
