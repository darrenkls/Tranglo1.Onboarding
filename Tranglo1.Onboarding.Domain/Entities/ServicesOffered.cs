using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ServicesOffered : Enumeration
    {
        public ServicesOffered() : base() { }

        public ServicesOffered(int id, string name) : base(id, name) { }

        public static readonly ServicesOffered MoneyTransfer_or_Remittance = new ServicesOffered(1, "Money Transfer / Remittance");
        public static readonly ServicesOffered ForeignCurrencyExchange = new ServicesOffered(2, "Foreign Currency Exchange");
        public static readonly ServicesOffered Retail_or_CommercialBankingServices = new ServicesOffered(3, "Retail / Commercial Banking Services");
        public static readonly ServicesOffered ForexTrading = new ServicesOffered(4, "Forex Trading");
        public static readonly ServicesOffered EMoney_or_EWallet = new ServicesOffered(5, "E-Money / E-Wallet");
        public static readonly ServicesOffered IntermediaryRemittance = new ServicesOffered(6, "Intermediary Remittance");
        public static readonly ServicesOffered Cryptocurrency = new ServicesOffered(7, "Cryptocurrency");
        public static readonly ServicesOffered Other = new ServicesOffered(8, "Other");
    }
}
