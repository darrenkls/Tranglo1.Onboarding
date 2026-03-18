using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DocumentTemplateType : Enumeration
    {
        public DocumentTemplateType() : base()
        {
        }

        public DocumentTemplateType(int id, string name)
            : base(id, name)
        {

        }

        public static readonly DocumentTemplateType General = new DocumentTemplateType(1, "General");
        public static readonly DocumentTemplateType Bank = new DocumentTemplateType(2, "Standard");
        public static readonly DocumentTemplateType NonBank = new DocumentTemplateType(3, "Non-Bank Payout");
        public static readonly DocumentTemplateType IntermediaryRemittance = new DocumentTemplateType(4, "Intermediary");
        public static readonly DocumentTemplateType CryptoExchange = new DocumentTemplateType(5, "Crypto Exchange");
    }
}
