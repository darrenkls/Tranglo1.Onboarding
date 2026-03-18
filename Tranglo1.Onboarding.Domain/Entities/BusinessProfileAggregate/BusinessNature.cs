using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class BusinessNature : Enumeration
    {
        public BusinessNature() : base()
        {
        }

        public BusinessNature(int id, string name)
            : base(id, name)
        {

        }

        public static readonly BusinessNature AccountingFirm = new BusinessNature(1, "Accounting Services");
        public static readonly BusinessNature Agriculture = new BusinessNature(2, "Agricultural Business");
        public static readonly BusinessNature Associations_Clubs = new BusinessNature(3, "Associations and Clubs");
        public static readonly BusinessNature Automobile_Parts_Dealer = new BusinessNature(4, "Motor Vehicle & Parts Dealer");
        //public static readonly BusinessNature Automobile_Vehicle_Dealer = new BusinessNature(5, "Automobile Vehicle Dealer");
        public static readonly BusinessNature Banking_Financial = new BusinessNature(6, "Banking / Financial Services");
        //public static readonly BusinessNature Casino_Gambling = new BusinessNature(7, "Casino / Gambling");
        public static readonly BusinessNature Charity_Humanitarian_Organisation = new BusinessNature(8, "Charitable and Humanitarian Organisation");
        public static readonly BusinessNature Construction = new BusinessNature(9, "Construction Services");
        public static readonly BusinessNature eCommerce = new BusinessNature(10, "E-Commerce Business");
        public static readonly BusinessNature Education = new BusinessNature(11, "Education");
        public static readonly BusinessNature Energy = new BusinessNature(12, "Energy (Oil, Gas & Utilities)");
        public static readonly BusinessNature Engineering_Chemical = new BusinessNature(13, "Engineering and Chemical Services");
        public static readonly BusinessNature Entertainment = new BusinessNature(14, "Entertainment");
        public static readonly BusinessNature Food_Beverages = new BusinessNature(15, "Food & Beverages");
        public static readonly BusinessNature Blocked = new BusinessNature(16, "IT Hardware & Technology Services");
        public static readonly BusinessNature Hardware_Technology = new BusinessNature(17, "Hotel / Resorts");
        public static readonly BusinessNature Law_Firm = new BusinessNature(18, "Legal Services");
        public static readonly BusinessNature Manufacturing = new BusinessNature(19, "Manufacturing");
        public static readonly BusinessNature Medical_Healthcare = new BusinessNature(20, "Medical / Healthcare");
        public static readonly BusinessNature Plantation = new BusinessNature(21, "Agriculture & Plantation");
        public static readonly BusinessNature Precious_Metal = new BusinessNature(22, "Jewellery & Precious Metals Trading");
        public static readonly BusinessNature Registered_State_Agents = new BusinessNature(23, "Real Estate Agency Services");
        //public static readonly BusinessNature Services = new BusinessNature(24, "Services");
        public static readonly BusinessNature Software_Technology = new BusinessNature(25, "Software Development Services");
        //public static readonly BusinessNature Tobacco_Cigarette = new BusinessNature(26, "Tobacco / Cigarette Dealer");
        //public static readonly BusinessNature Trading = new BusinessNature(27, "Trading");
        public static readonly BusinessNature Travel_Agency = new BusinessNature(28, "Travel Agency Services");
        public static readonly BusinessNature Trust_Foundation = new BusinessNature(29, "Trusts and Foundations");
        //public static readonly BusinessNature Utility = new BusinessNature(30, "Utility (i.e.: electrical power)");
        public static readonly BusinessNature Other = new BusinessNature(31, "Others");
        //public static readonly BusinessNature Night_club_pubs = new BusinessNature(32, "Night club / pubs");
        //public static readonly BusinessNature Money_services_business = new BusinessNature(33, "Money services business");
        //public static readonly BusinessNature Liquor_distributor_dealer = new BusinessNature(34, "Liquor distributor / dealer");
        //public static readonly BusinessNature Money_lenders = new BusinessNature(35, "Money lenders");
        public static readonly BusinessNature Notaries_public = new BusinessNature(36, "Legal/Administration Services");
        public static readonly BusinessNature Pawnbrokers = new BusinessNature(37, "Pawnbroking Services");
        //public static readonly BusinessNature Company_secretaries = new BusinessNature(38, "Company secretaries");
        public static readonly BusinessNature Beauty_Wellness_Services = new BusinessNature(39, "Beauty & Wellness Services");
        public static readonly BusinessNature Business_Management_Consultancy = new BusinessNature(40, "Business & Management Consultancy");
        public static readonly BusinessNature Information_Communications_Technology_ICT_Services = new BusinessNature(41, "Information & Communications Technology (ICT) Services");
        public static readonly BusinessNature Manufacuting_Dual_Used_Goods  = new BusinessNature(42, "Manufacturing of Strategic / Dual-use Goods");
        public static readonly BusinessNature Trading_of_Strategic_Dual_use_Goods = new BusinessNature(43, "Trading of Strategic / Dual-use Goods");
        public static readonly BusinessNature Transportation_and_Logistics_Services = new BusinessNature(44, "Transportation and Logistics Services");
        public static readonly BusinessNature Wholesale_Retail_Trade = new BusinessNature(45, "Wholesale & Retail Trade");

    }
}
