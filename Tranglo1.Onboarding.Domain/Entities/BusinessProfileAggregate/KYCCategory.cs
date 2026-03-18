using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCCategory : Enumeration
    {
        public long SolutionCode { get; set; }
        public string PortalDisplayName { get; set; }

        public KYCCategory() : base()
        {
        }

        public KYCCategory(int id, string name, long solutionCode, string portalDisplayName)
            : base(id, name)
        {
            this.SolutionCode = solutionCode;
            this.PortalDisplayName = portalDisplayName;
        }
        /*
        Derive data from Compliance Officer’s review 
        •	Insufficient / Incomplete
        User need to take action to complete KYC based on Compliance Officer’s feedback and review results
        •	Complete
        Compliance Officer has approved user’s KYC
        */
        public static readonly KYCCategory Connect_BusinessProfile = new KYCCategory(1, "Business Profile", Solution.Connect.Id, "Business Profile"); //Default value 
        public static readonly KYCCategory Connect_LicenseInfo = new KYCCategory(2, "License Information", Solution.Connect.Id, "License Information");
        public static readonly KYCCategory Connect_Ownership = new KYCCategory(3, "Ownership and Management Structure", Solution.Connect.Id, "Organisational Structure");
        public static readonly KYCCategory Connect_Documentation = new KYCCategory(4, "Documentation", Solution.Connect.Id, "Supporting Documents");
        public static readonly KYCCategory Connect_AMLOrCFT = new KYCCategory(5, "AML/CFT Questionnaire", Solution.Connect.Id, "Due Diligence Questionnaire");
        public static readonly KYCCategory Connect_ComplianceInfo = new KYCCategory(6, "Compliance Officer Information", Solution.Connect.Id, "CO Information");
        public static readonly KYCCategory Connect_Declaration = new KYCCategory(7, "Declaration", Solution.Connect.Id, "Declaration");

        //Future can enhanced the portal display name in TB
        public static readonly KYCCategory Business_BusinessDeclaration = new KYCCategory(8, "Business Declaration", Solution.Business.Id, "Business Declaration");
        public static readonly KYCCategory Business_BusinessProfile = new KYCCategory(9, "Business Profile", Solution.Business.Id, "Business Profile");
        public static readonly KYCCategory Business_LicenseInfo = new KYCCategory(10, "License Information", Solution.Business.Id, "License Information");
        public static readonly KYCCategory Business_Ownership = new KYCCategory(11, "Ownership and Management Structure", Solution.Business.Id, "Ownership and Management Structure");
        public static readonly KYCCategory Business_TransactionEvaluation = new KYCCategory(12, "Transaction Evaluation", Solution.Business.Id, "Transaction Evaluation");
        public static readonly KYCCategory Business_Verification = new KYCCategory(13, "Verification", Solution.Business.Id, "Verification");
        public static readonly KYCCategory Business_Documentation = new KYCCategory(14, "Documentation", Solution.Business.Id, "Documentation");
        public static readonly KYCCategory Business_AMLOrCFT = new KYCCategory(15, "AML/CFT Questionnaire", Solution.Business.Id, "AML/CFT Questionnaire");
        public static readonly KYCCategory Business_ComplianceInfo = new KYCCategory(16, "Compliance Officer Information", Solution.Business.Id, "Compliance Officer Information");
        public static readonly KYCCategory Business_Declaration = new KYCCategory(17, "Declaration", Solution.Business.Id, "Declaration");


        public static List<KYCCategory> GetBusinessAdminAllowedMainCategories(CustomerType customerType, TrangloEntity trangloEntity)
        {
            var mainCategories = new List<KYCCategory>();

            if (customerType.IsIndividual())
            {
                mainCategories.Add(Business_BusinessDeclaration);
                mainCategories.Add(Business_BusinessProfile);
                mainCategories.Add(Business_TransactionEvaluation);
                mainCategories.Add(Business_Verification);
                mainCategories.Add(Business_Documentation);
                mainCategories.Add(Business_Declaration);
            }
            else if (customerType.IsCryptocurrency() || customerType.IsRemittancePartner())
            {
                mainCategories.Add(Business_BusinessDeclaration);
                mainCategories.Add(Business_BusinessProfile);
                mainCategories.Add(Business_LicenseInfo);
                mainCategories.Add(Business_Ownership);
                if (trangloEntity == TrangloEntity.TSB)
                {
                    mainCategories.Add(Business_Verification);
                }
                mainCategories.Add(Business_Documentation);
                mainCategories.Add(Business_AMLOrCFT);
                mainCategories.Add(Business_ComplianceInfo);
                mainCategories.Add(Business_Declaration);
            }
            else if (customerType.IsCorporate())
            {
                mainCategories.Add(Business_BusinessProfile);
                mainCategories.Add(Business_LicenseInfo);
                mainCategories.Add(Business_Ownership);
                mainCategories.Add(Business_TransactionEvaluation);
                if (trangloEntity == TrangloEntity.TSB)
                {
                    mainCategories.Add(Business_Verification);
                }
                mainCategories.Add(Business_Documentation);
                mainCategories.Add(Business_Declaration);
            }

            return mainCategories;
        }

        public static List<KYCCategory> GetBusinessCustomerUserAllowedMainCategories(CustomerType customerType, TrangloEntity trangloEntity)
        {
            var mainCategories = new List<KYCCategory>();

            if (customerType.IsIndividual())
            {
                mainCategories.Add(Business_BusinessProfile);
                mainCategories.Add(Business_TransactionEvaluation);
                mainCategories.Add(Business_Verification);
                mainCategories.Add(Business_Documentation);
                mainCategories.Add(Business_Declaration);
            }
            else if (customerType.IsCryptocurrency() || customerType.IsRemittancePartner())
            {
                mainCategories.Add(Business_BusinessProfile);
                mainCategories.Add(Business_LicenseInfo);
                mainCategories.Add(Business_Ownership);
                if (trangloEntity == TrangloEntity.TSB)
                {
                    mainCategories.Add(Business_Verification);
                }
                mainCategories.Add(Business_Documentation);
                mainCategories.Add(Business_AMLOrCFT);
                mainCategories.Add(Business_ComplianceInfo);
                mainCategories.Add(Business_Declaration);
            }
            else if (customerType.IsCorporate())
            {
                mainCategories.Add(Business_BusinessProfile);
                mainCategories.Add(Business_LicenseInfo);
                mainCategories.Add(Business_Ownership);
                mainCategories.Add(Business_TransactionEvaluation);
                if (trangloEntity == TrangloEntity.TSB)
                {
                    mainCategories.Add(Business_Verification);
                }
                mainCategories.Add(Business_Documentation);
                mainCategories.Add(Business_Declaration);
            }

            return mainCategories;
        }
    }
}
