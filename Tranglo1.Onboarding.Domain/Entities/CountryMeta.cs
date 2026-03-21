using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CountryMeta : Enumeration
    {
        public string CountryISO2 { get; set; }
        public string CountryISO3 { get; set; }
        public string DialCode { get; set; }

        public CountryMeta() : base() { }

        public CountryMeta(int id, string name, string countryISO2, string countryISO3, string dialCode)
            : base(id, name)
        {
            CountryISO2 = countryISO2;
            CountryISO3 = countryISO3;
            DialCode = dialCode;
        }

        public static IEnumerable<CountryMeta> GetAllCountryMeta()
        {
            return _CachedEnumerations.GetOrAdd(typeof(CountryMeta), t =>
            {
                var fields = typeof(CountryMeta).GetFields(BindingFlags.Public |
                                                           BindingFlags.Static |
                                                           BindingFlags.DeclaredOnly);
                return fields
                    .Where(f => f.DeclaringType == t)
                    .Select(f => f.GetValue(null))
                    .Cast<CountryMeta>()
                    .OrderBy(e => e.Name)
                    .ToArray();
            }).Cast<CountryMeta>();
        }

        public static CountryMeta GetCountryByISO2Async(string countryISO2)
        {
            var countryUpper = countryISO2?.ToUpper();
            return GetAllCountryMeta().FirstOrDefault(x => x.CountryISO2 == countryUpper);
        }

        public static CountryMeta GetCountryByDialCodeAsync(string dialCode)
        {
            return GetAllCountryMeta().FirstOrDefault(x => x.DialCode == dialCode);
        }

        public static string GetCountryISO2ByIdAsync(long countryCode)
        {
            return GetAllCountryMeta().FirstOrDefault(x => x.Id == countryCode)?.CountryISO2;
        }

        public static readonly CountryMeta Afghanistan = new CountryMeta(1, "Afghanistan", "AF", "AFG", "+93");
        public static readonly CountryMeta Åland_Islands = new CountryMeta(2, "Åland Islands", "AX", "ALA", "+358");
        public static readonly CountryMeta Albania = new CountryMeta(3, "Albania", "AL", "ALB", "+355");
        public static readonly CountryMeta Algeria = new CountryMeta(4, "Algeria", "DZ", "DZA", "+213");
        public static readonly CountryMeta American_Samoa = new CountryMeta(5, "American Samoa", "AS", "ASM", "+1684");
        public static readonly CountryMeta Andorra = new CountryMeta(6, "Andorra", "AD", "AND", "+376");
        public static readonly CountryMeta Angola = new CountryMeta(7, "Angola", "AO", "AGO", "+244");
        public static readonly CountryMeta Anguilla = new CountryMeta(8, "Anguilla", "AI", "AIA", "+1264");
        public static readonly CountryMeta Antarctica = new CountryMeta(9, "Antarctica", "AQ", "ATA", "+6721");
        public static readonly CountryMeta Antigua_and_Barbuda = new CountryMeta(10, "Antigua and Barbuda", "AG", "ATG", "+1268");
        public static readonly CountryMeta Argentina = new CountryMeta(11, "Argentina", "AR", "ARG", "+54");
        public static readonly CountryMeta Armenia = new CountryMeta(12, "Armenia", "AM", "ARM", "+374");
        public static readonly CountryMeta Aruba = new CountryMeta(13, "Aruba", "AW", "ABW", "+297");
        public static readonly CountryMeta Australia = new CountryMeta(14, "Australia", "AU", "AUS", "+61");
        public static readonly CountryMeta Austria = new CountryMeta(15, "Austria", "AT", "AUT", "+43");
        public static readonly CountryMeta Azerbaijan = new CountryMeta(16, "Azerbaijan", "AZ", "AZE", "+994");
        public static readonly CountryMeta Bahamas = new CountryMeta(17, "Bahamas", "BS", "BHS", "+1242");
        public static readonly CountryMeta Bahrain = new CountryMeta(18, "Bahrain", "BH", "BHR", "+973");
        public static readonly CountryMeta Bangladesh = new CountryMeta(19, "Bangladesh", "BD", "BGD", "+880");
        public static readonly CountryMeta Barbados = new CountryMeta(20, "Barbados", "BB", "BRB", "+1246");
        public static readonly CountryMeta Belarus = new CountryMeta(21, "Belarus", "BY", "BLR", "+375");
        public static readonly CountryMeta Belgium = new CountryMeta(22, "Belgium", "BE", "BEL", "+32");
        public static readonly CountryMeta Belize = new CountryMeta(23, "Belize", "BZ", "BLZ", "+501");
        public static readonly CountryMeta Benin = new CountryMeta(24, "Benin", "BJ", "BEN", "+229");
        public static readonly CountryMeta Bermuda = new CountryMeta(25, "Bermuda", "BM", "BMU", "+1441");
        public static readonly CountryMeta Bhutan = new CountryMeta(26, "Bhutan", "BT", "BTN", "+975");
        public static readonly CountryMeta Bolivia = new CountryMeta(27, "Bolivia", "BO", "BOL", "+591");
        public static readonly CountryMeta Bonaire_Sint_Eustatius_and_Saba = new CountryMeta(28, "Bonaire, Sint Eustatius and Saba", "BQ", "BES", "+599");
        public static readonly CountryMeta Bosnia_and_Herzegovina = new CountryMeta(29, "Bosnia and Herzegovina", "BA", "BIH", "+387");
        public static readonly CountryMeta Botswana = new CountryMeta(30, "Botswana", "BW", "BWA", "+267");
        public static readonly CountryMeta Bouvet_Island = new CountryMeta(31, "Bouvet Island", "BV", "BVT", "+601");
        public static readonly CountryMeta Brazil = new CountryMeta(32, "Brazil", "BR", "BRA", "+55");
        public static readonly CountryMeta British_Indian_Ocean_Territory = new CountryMeta(33, "British Indian Ocean Territory", "IO", "IOT", "+246");
        public static readonly CountryMeta Brunei_Darussalam = new CountryMeta(34, "Brunei Darussalam", "BN", "BRN", "+673");
        public static readonly CountryMeta Bulgaria = new CountryMeta(35, "Bulgaria", "BG", "BGR", "+359");
        public static readonly CountryMeta Burkina_Faso = new CountryMeta(36, "Burkina Faso", "BF", "BFA", "+226");
        public static readonly CountryMeta Burundi = new CountryMeta(37, "Burundi", "BI", "BDI", "+257");
        public static readonly CountryMeta Cambodia = new CountryMeta(38, "Cambodia", "KH", "KHM", "+855");
        public static readonly CountryMeta Cameroon = new CountryMeta(39, "Cameroon", "CM", "CMR", "+237");
        public static readonly CountryMeta Canada = new CountryMeta(40, "Canada", "CA", "CAN", "+1");
        public static readonly CountryMeta Cape_Verde = new CountryMeta(41, "Cape Verde", "CV", "CPV", "+238");
        public static readonly CountryMeta Cayman_Islands = new CountryMeta(42, "Cayman Islands", "KY", "CYM", "+1345");
        public static readonly CountryMeta Central_African_Republic = new CountryMeta(43, "Central African Republic", "CF", "CAF", "+236");
        public static readonly CountryMeta Chad = new CountryMeta(44, "Chad", "TD", "TCD", "+235");
        public static readonly CountryMeta Chile = new CountryMeta(45, "Chile", "CL", "CHL", "+56");
        public static readonly CountryMeta China = new CountryMeta(46, "China", "CN", "CHN", "+86");
        public static readonly CountryMeta Christmas_Island = new CountryMeta(47, "Christmas Island", "CX", "CXR", "+618");
        public static readonly CountryMeta Cocos_Keeling_Islands = new CountryMeta(48, "Cocos (Keeling) Islands", "CC", "CCK", "+618");
        public static readonly CountryMeta Colombia = new CountryMeta(49, "Colombia", "CO", "COL", "+57");
        public static readonly CountryMeta Comoros = new CountryMeta(50, "Comoros", "KM", "COM", "+269");
        public static readonly CountryMeta Congo = new CountryMeta(51, "Republic of the Congo", "CG", "COG", "+242");
        public static readonly CountryMeta Congo_the_Democratic_Republic_of_the = new CountryMeta(52, "Democratic Republic of the Congo", "CD", "COD", "+243");
        public static readonly CountryMeta Cook_Islands = new CountryMeta(53, "Cook Islands", "CK", "COK", "+682");
        public static readonly CountryMeta Costa_Rica = new CountryMeta(54, "Costa Rica", "CR", "CRI", "+506");
        public static readonly CountryMeta Cote_DIvoire = new CountryMeta(55, "Cote D'Ivoire", "CI", "CIV", "+225");
        public static readonly CountryMeta Croatia = new CountryMeta(56, "Croatia", "HR", "HRV", "+385");
        public static readonly CountryMeta Cuba = new CountryMeta(57, "Cuba", "CU", "CUB", "+53");
        public static readonly CountryMeta Curaao = new CountryMeta(58, "Curaçao", "CW", "CUW", "+599");
        public static readonly CountryMeta Cyprus = new CountryMeta(59, "Cyprus", "CY", "CYP", "+357");
        public static readonly CountryMeta Czech_Republic = new CountryMeta(60, "Czech Republic", "CZ", "CZE", "+420");
        public static readonly CountryMeta Denmark = new CountryMeta(61, "Denmark", "DK", "DNK", "+45");
        public static readonly CountryMeta Djibouti = new CountryMeta(62, "Djibouti", "DJ", "DJI", "+253");
        public static readonly CountryMeta Dominica = new CountryMeta(63, "Dominica", "DM", "DMA", "+1767");
        public static readonly CountryMeta Dominican_Republic = new CountryMeta(64, "Dominican Republic", "DO", "DOM", "+1809");
        public static readonly CountryMeta Ecuador = new CountryMeta(65, "Ecuador", "EC", "ECU", "+593");
        public static readonly CountryMeta Egypt = new CountryMeta(66, "Egypt", "EG", "EGY", "+20");
        public static readonly CountryMeta El_Salvador = new CountryMeta(67, "El Salvador", "SV", "SLV", "+503");
        public static readonly CountryMeta Equatorial_Guinea = new CountryMeta(68, "Equatorial Guinea", "GQ", "GNQ", "+240");
        public static readonly CountryMeta Eritrea = new CountryMeta(69, "Eritrea", "ER", "ERI", "+291");
        public static readonly CountryMeta Estonia = new CountryMeta(70, "Estonia", "EE", "EST", "+372");
        public static readonly CountryMeta Ethiopia = new CountryMeta(71, "Ethiopia", "ET", "ETH", "+251");
        public static readonly CountryMeta Falkland_Islands_Malvinas = new CountryMeta(72, "Falkland Islands (Malvinas)", "FK", "FLK", "+500");
        public static readonly CountryMeta Faroe_Islands = new CountryMeta(73, "Faroe Islands", "FO", "FRO", "+298");
        public static readonly CountryMeta Fiji = new CountryMeta(74, "Fiji", "FJ", "FJI", "+679");
        public static readonly CountryMeta Finland = new CountryMeta(75, "Finland", "FI", "FIN", "+358");
        public static readonly CountryMeta France = new CountryMeta(76, "France", "FR", "FRA", "+33");
        public static readonly CountryMeta French_Guiana = new CountryMeta(77, "French Guiana", "GF", "GUF", "+594");
        public static readonly CountryMeta French_Polynesia = new CountryMeta(78, "French Polynesia", "PF", "PYF", "+689");
        public static readonly CountryMeta French_Southern_Territories = new CountryMeta(79, "French Southern Territories", "TF", "ATF", "+601");
        public static readonly CountryMeta Gabon = new CountryMeta(80, "Gabon", "GA", "GAB", "+241");
        public static readonly CountryMeta Gambia = new CountryMeta(81, "Gambia", "GM", "GMB", "+220");
        public static readonly CountryMeta Georgia = new CountryMeta(82, "Georgia", "GE", "GEO", "+995");
        public static readonly CountryMeta Germany = new CountryMeta(83, "Germany", "DE", "DEU", "+49");
        public static readonly CountryMeta Ghana = new CountryMeta(84, "Ghana", "GH", "GHA", "+233");
        public static readonly CountryMeta Gibraltar = new CountryMeta(85, "Gibraltar", "GI", "GIB", "+350");
        public static readonly CountryMeta Greece = new CountryMeta(86, "Greece", "GR", "GRC", "+30");
        public static readonly CountryMeta Greenland = new CountryMeta(87, "Greenland", "GL", "GRL", "+299");
        public static readonly CountryMeta Grenada = new CountryMeta(88, "Grenada", "GD", "GRD", "+1473");
        public static readonly CountryMeta Guadeloupe = new CountryMeta(89, "Guadeloupe", "GP", "GLP", "+590");
        public static readonly CountryMeta Guam = new CountryMeta(90, "Guam", "GU", "GUM", "+1671");
        public static readonly CountryMeta Guatemala = new CountryMeta(91, "Guatemala", "GT", "GTM", "+502");
        public static readonly CountryMeta Guernsey = new CountryMeta(92, "Guernsey", "GG", "GGY", "+44");
        public static readonly CountryMeta Guinea = new CountryMeta(93, "Guinea", "GN", "GIN", "+224");
        public static readonly CountryMeta GuineaBissau = new CountryMeta(94, "Guinea-Bissau", "GW", "GNB", "+245");
        public static readonly CountryMeta Guyana = new CountryMeta(95, "Guyana", "GY", "GUY", "+592");
        public static readonly CountryMeta Haiti = new CountryMeta(96, "Haiti", "HT", "HTI", "+509");
        public static readonly CountryMeta Heard_Island_and_Mcdonald_Islands = new CountryMeta(97, "Heard Island and Mcdonald Islands", "HM", "HMD", "+601");
        public static readonly CountryMeta Holy_See_Vatican_City_State = new CountryMeta(98, "Holy See (Vatican City State)", "VA", "VAT", "+379");
        public static readonly CountryMeta Honduras = new CountryMeta(99, "Honduras", "HN", "HND", "+504");
        public static readonly CountryMeta Hong_Kong = new CountryMeta(100, "Hong Kong", "HK", "HKG", "+852");
        public static readonly CountryMeta Hungary = new CountryMeta(101, "Hungary", "HU", "HUN", "+36");
        public static readonly CountryMeta Iceland = new CountryMeta(102, "Iceland", "IS", "ISL", "+354");
        public static readonly CountryMeta India = new CountryMeta(103, "India", "IN", "IND", "+91");
        public static readonly CountryMeta Indonesia = new CountryMeta(104, "Indonesia", "ID", "IDN", "+62");
        public static readonly CountryMeta Iran_Islamic_Republic_of = new CountryMeta(105, "Iran, Islamic Republic of", "IR", "IRN", "+98");
        public static readonly CountryMeta Iraq = new CountryMeta(106, "Iraq", "IQ", "IRQ", "+964");
        public static readonly CountryMeta Ireland = new CountryMeta(107, "Ireland", "IE", "IRL", "+353");
        public static readonly CountryMeta Isle_of_Man = new CountryMeta(108, "Isle of Man", "IM", "IMN", "+44");
        public static readonly CountryMeta Israel = new CountryMeta(109, "Israel", "IL", "ISR", "+972");
        public static readonly CountryMeta Italy = new CountryMeta(110, "Italy", "IT", "ITA", "+39");
        public static readonly CountryMeta Jamaica = new CountryMeta(111, "Jamaica", "JM", "JAM", "+1876");
        public static readonly CountryMeta Japan = new CountryMeta(112, "Japan", "JP", "JPN", "+81");
        public static readonly CountryMeta Jersey = new CountryMeta(113, "Jersey", "JE", "JEY", "+44");
        public static readonly CountryMeta Jordan = new CountryMeta(114, "Jordan", "JO", "JOR", "+962");
        public static readonly CountryMeta Kazakhstan = new CountryMeta(115, "Kazakhstan", "KZ", "KAZ", "+77");
        public static readonly CountryMeta Kenya = new CountryMeta(116, "Kenya", "KE", "KEN", "+254");
        public static readonly CountryMeta Kiribati = new CountryMeta(117, "Kiribati", "KI", "KIR", "+686");
        public static readonly CountryMeta Korea_Democratic_Peoples_Republic_of = new CountryMeta(118, "North Korea (Democratic People's Republic of)", "KP", "PRK", "+850");
        public static readonly CountryMeta Korea_Republic_of = new CountryMeta(119, "Korea, Republic of", "KR", "KOR", "+82");
        public static readonly CountryMeta Kuwait = new CountryMeta(120, "Kuwait", "KW", "KWT", "+965");
        public static readonly CountryMeta Kyrgyzstan = new CountryMeta(121, "Kyrgyzstan", "KG", "KGZ", "+996");
        public static readonly CountryMeta Lao_Peoples_Democratic_Republic = new CountryMeta(122, "Lao People's Democratic Republic", "LA", "LAO", "+856");
        public static readonly CountryMeta Latvia = new CountryMeta(123, "Latvia", "LV", "LVA", "+371");
        public static readonly CountryMeta Lebanon = new CountryMeta(124, "Lebanon", "LB", "LBN", "+961");
        public static readonly CountryMeta Lesotho = new CountryMeta(125, "Lesotho", "LS", "LSO", "+266");
        public static readonly CountryMeta Liberia = new CountryMeta(126, "Liberia", "LR", "LBR", "+231");
        public static readonly CountryMeta Libyan_Arab_Jamahiriya = new CountryMeta(127, "Libyan Arab Jamahiriya", "LY", "LBY", "+218");
        public static readonly CountryMeta Liechtenstein = new CountryMeta(128, "Liechtenstein", "LI", "LIE", "+423");
        public static readonly CountryMeta Lithuania = new CountryMeta(129, "Lithuania", "LT", "LTU", "+370");
        public static readonly CountryMeta Luxembourg = new CountryMeta(130, "Luxembourg", "LU", "LUX", "+352");
        public static readonly CountryMeta Macao = new CountryMeta(131, "Macao", "MO", "MAC", "+853");
        public static readonly CountryMeta Macedonia_the_Former_Yugoslav_Republic_of = new CountryMeta(132, "Macedonia, the Former Yugoslav Republic of", "MK", "MKD", "+389");
        public static readonly CountryMeta Madagascar = new CountryMeta(133, "Madagascar", "MG", "MDG", "+261");
        public static readonly CountryMeta Malawi = new CountryMeta(134, "Malawi", "MW", "MWI", "+265");
        public static readonly CountryMeta Malaysia = new CountryMeta(135, "Malaysia", "MY", "MYS", "+60");
        public static readonly CountryMeta Maldives = new CountryMeta(136, "Maldives", "MV", "MDV", "+960");
        public static readonly CountryMeta Mali = new CountryMeta(137, "Mali", "ML", "MLI", "+223");
        public static readonly CountryMeta Malta = new CountryMeta(138, "Malta", "MT", "MLT", "+356");
        public static readonly CountryMeta Marshall_Islands = new CountryMeta(139, "Marshall Islands", "MH", "MHL", "+692");
        public static readonly CountryMeta Martinique = new CountryMeta(140, "Martinique", "MQ", "MTQ", "+596");
        public static readonly CountryMeta Mauritania = new CountryMeta(141, "Mauritania", "MR", "MRT", "+222");
        public static readonly CountryMeta Mauritius = new CountryMeta(142, "Mauritius", "MU", "MUS", "+230");
        public static readonly CountryMeta Mayotte = new CountryMeta(143, "Mayotte", "YT", "MYT", "+262");
        public static readonly CountryMeta Mexico = new CountryMeta(144, "Mexico", "MX", "MEX", "+52");
        public static readonly CountryMeta Micronesia_Federated_States_of = new CountryMeta(145, "Micronesia, Federated States of", "FM", "FSM", "+373");
        public static readonly CountryMeta Moldova_Republic_of = new CountryMeta(146, "Moldova, Republic of", "MD", "MDA", "+373");
        public static readonly CountryMeta Monaco = new CountryMeta(147, "Monaco", "MC", "MCO", "+377");
        public static readonly CountryMeta Mongolia = new CountryMeta(148, "Mongolia", "MN", "MNG", "+976");
        public static readonly CountryMeta Montenegro = new CountryMeta(149, "Montenegro", "ME", "MNE", "+382");
        public static readonly CountryMeta Montserrat = new CountryMeta(150, "Montserrat", "MS", "MSR", "+1664");
        public static readonly CountryMeta Morocco = new CountryMeta(151, "Morocco", "MA", "MAR", "+212");
        public static readonly CountryMeta Mozambique = new CountryMeta(152, "Mozambique", "MZ", "MOZ", "+258");
        public static readonly CountryMeta Myanmar = new CountryMeta(153, "Myanmar", "MM", "MMR", "+95");
        public static readonly CountryMeta Namibia = new CountryMeta(154, "Namibia", "NA", "NAM", "+264");
        public static readonly CountryMeta Nauru = new CountryMeta(155, "Nauru", "NR", "NRU", "+674");
        public static readonly CountryMeta Nepal = new CountryMeta(156, "Nepal", "NP", "NPL", "+977");
        public static readonly CountryMeta Netherlands = new CountryMeta(157, "Netherlands", "NL", "NLD", "+31");
        public static readonly CountryMeta New_Caledonia = new CountryMeta(159, "New Caledonia", "NC", "NCL", "+687");
        public static readonly CountryMeta New_Zealand = new CountryMeta(160, "New Zealand", "NZ", "NZL", "+64");
        public static readonly CountryMeta Nicaragua = new CountryMeta(161, "Nicaragua", "NI", "NIC", "+505");
        public static readonly CountryMeta Niger = new CountryMeta(162, "Niger", "NE", "NER", "+227");
        public static readonly CountryMeta Nigeria = new CountryMeta(163, "Nigeria", "NG", "NGA", "+234");
        public static readonly CountryMeta Niue = new CountryMeta(164, "Niue", "NU", "NIU", "+683");
        public static readonly CountryMeta Norfolk_Island = new CountryMeta(165, "Norfolk Island", "NF", "NFK", "+672");
        public static readonly CountryMeta Northern_Mariana_Islands = new CountryMeta(166, "Northern Mariana Islands", "MP", "MNP", "+1670");
        public static readonly CountryMeta Norway = new CountryMeta(167, "Norway", "NO", "NOR", "+47");
        public static readonly CountryMeta Oman = new CountryMeta(168, "Oman", "OM", "OMN", "+968");
        public static readonly CountryMeta Pakistan = new CountryMeta(169, "Pakistan", "PK", "PAK", "+92");
        public static readonly CountryMeta Palau = new CountryMeta(170, "Palau", "PW", "PLW", "+680");
        public static readonly CountryMeta Palestinian_Territory_Occupied = new CountryMeta(171, "Palestinian Territory, Occupied", "PS", "PSE", "+970");
        public static readonly CountryMeta Panama = new CountryMeta(172, "Panama", "PA", "PAN", "+507");
        public static readonly CountryMeta Papua_New_Guinea = new CountryMeta(173, "Papua New Guinea", "PG", "PNG", "+675");
        public static readonly CountryMeta Paraguay = new CountryMeta(174, "Paraguay", "PY", "PRY", "+595");
        public static readonly CountryMeta Peru = new CountryMeta(175, "Peru", "PE", "PER", "+51");
        public static readonly CountryMeta Philippines = new CountryMeta(176, "Philippines", "PH", "PHL", "+63");
        public static readonly CountryMeta Pitcairn = new CountryMeta(177, "Pitcairn", "PN", "PCN", "+601");
        public static readonly CountryMeta Poland = new CountryMeta(178, "Poland", "PL", "POL", "+48");
        public static readonly CountryMeta Portugal = new CountryMeta(179, "Portugal", "PT", "PRT", "+351");
        public static readonly CountryMeta Puerto_Rico = new CountryMeta(180, "Puerto Rico", "PR", "PRI", "+1787");
        public static readonly CountryMeta Qatar = new CountryMeta(181, "Qatar", "QA", "QAT", "+974");
        public static readonly CountryMeta Reunion = new CountryMeta(182, "Reunion", "RE", "REU", "+262");
        public static readonly CountryMeta Romania = new CountryMeta(183, "Romania", "RO", "ROM", "+40");
        public static readonly CountryMeta Russian_Federation = new CountryMeta(184, "Russian Federation", "RU", "RUS", "+7");
        public static readonly CountryMeta Rwanda = new CountryMeta(185, "Rwanda", "RW", "RWA", "+250");
        public static readonly CountryMeta Saint_Barthlemy = new CountryMeta(186, "Saint Barthélemy", "BL", "BLM", "+590");
        public static readonly CountryMeta Saint_Helena = new CountryMeta(187, "Saint Helena", "SH", "SHN", "+290");
        public static readonly CountryMeta Saint_Kitts_and_Nevis = new CountryMeta(188, "Saint Kitts and Nevis", "KN", "KNA", "+1869");
        public static readonly CountryMeta Saint_Lucia = new CountryMeta(189, "Saint Lucia", "LC", "LCA", "+1758");
        public static readonly CountryMeta Saint_Martin = new CountryMeta(190, "Saint Martin", "MF", "MAF", "+590");
        public static readonly CountryMeta Saint_Pierre_and_Miquelon = new CountryMeta(191, "Saint Pierre and Miquelon", "PM", "SPM", "+508");
        public static readonly CountryMeta Saint_Vincent_and_the_Grenadines = new CountryMeta(192, "Saint Vincent and the Grenadines", "VC", "VCT", "+1784");
        public static readonly CountryMeta Samoa = new CountryMeta(193, "Samoa", "WS", "WSM", "+685");
        public static readonly CountryMeta San_Marino = new CountryMeta(194, "San Marino", "SM", "SMR", "+378");
        public static readonly CountryMeta Sao_Tome_and_Principe = new CountryMeta(195, "Sao Tome and Principe", "ST", "STP", "+239");
        public static readonly CountryMeta Saudi_Arabia = new CountryMeta(196, "Saudi Arabia", "SA", "SAU", "+966");
        public static readonly CountryMeta Senegal = new CountryMeta(197, "Senegal", "SN", "SEN", "+221");
        public static readonly CountryMeta Serbia_and_Montenegro = new CountryMeta(198, "Serbia and Montenegro", "RS", "SRB", "+381");
        public static readonly CountryMeta Seychelles = new CountryMeta(199, "Seychelles", "SC", "SYC", "+248");
        public static readonly CountryMeta Sierra_Leone = new CountryMeta(200, "Sierra Leone", "SL", "SLE", "+232");
        public static readonly CountryMeta Singapore = new CountryMeta(201, "Singapore", "SG", "SGP", "+65");
        public static readonly CountryMeta Sint_Maarten = new CountryMeta(202, "Sint Maarten", "SX", "SXM", "+1721");
        public static readonly CountryMeta Slovakia = new CountryMeta(203, "Slovakia", "SK", "SVK", "+421");
        public static readonly CountryMeta Slovenia = new CountryMeta(204, "Slovenia", "SI", "SVN", "+386");
        public static readonly CountryMeta Solomon_Islands = new CountryMeta(205, "Solomon Islands", "SB", "SLB", "+677");
        public static readonly CountryMeta Somalia = new CountryMeta(206, "Somalia", "SO", "SOM", "+252");
        public static readonly CountryMeta South_Africa = new CountryMeta(207, "South Africa", "ZA", "ZAF", "+27");
        public static readonly CountryMeta South_Georgia_and_the_South_Sandwich_Islands = new CountryMeta(208, "South Georgia and the South Sandwich Islands", "GS", "SGS", "+601");
        public static readonly CountryMeta South_Sudan = new CountryMeta(209, "South Sudan", "SS", "SSD", "+211");
        public static readonly CountryMeta Spain = new CountryMeta(210, "Spain", "ES", "ESP", "+34");
        public static readonly CountryMeta Sri_Lanka = new CountryMeta(211, "Sri Lanka", "LK", "LKA", "+94");
        public static readonly CountryMeta Sudan = new CountryMeta(212, "Sudan", "SD", "SDN", "+249");
        public static readonly CountryMeta Suriname = new CountryMeta(213, "Suriname", "SR", "SUR", "+597");
        public static readonly CountryMeta Svalbard_and_Jan_Mayen = new CountryMeta(214, "Svalbard and Jan Mayen", "SJ", "SJM", "+601");
        public static readonly CountryMeta Swaziland = new CountryMeta(215, "Swaziland", "SZ", "SWZ", "+268");
        public static readonly CountryMeta Sweden = new CountryMeta(216, "Sweden", "SE", "SWE", "+46");
        public static readonly CountryMeta Switzerland = new CountryMeta(217, "Switzerland", "CH", "CHE", "+41");
        public static readonly CountryMeta Syrian_Arab_Republic = new CountryMeta(218, "Syrian Arab Republic", "SY", "SYR", "+963");
        public static readonly CountryMeta Taiwan_Province_of_China = new CountryMeta(219, "Taiwan, Province of China", "TW", "TWN", "+886");
        public static readonly CountryMeta Tajikistan = new CountryMeta(220, "Tajikistan", "TJ", "TJK", "+992");
        public static readonly CountryMeta Tanzania_United_Republic_of = new CountryMeta(221, "Tanzania, United Republic of", "TZ", "TZA", "+255");
        public static readonly CountryMeta Thailand = new CountryMeta(222, "Thailand", "TH", "THA", "+66");
        public static readonly CountryMeta TimorLeste = new CountryMeta(223, "Timor-Leste", "TL", "TLS", "+670");
        public static readonly CountryMeta Togo = new CountryMeta(224, "Togo", "TG", "TGO", "+228");
        public static readonly CountryMeta Tokelau = new CountryMeta(225, "Tokelau", "TK", "TKL", "+690");
        public static readonly CountryMeta Tonga = new CountryMeta(226, "Tonga", "TO", "TON", "+676");
        public static readonly CountryMeta Trinidad_and_Tobago = new CountryMeta(227, "Trinidad and Tobago", "TT", "TTO", "+1868");
        public static readonly CountryMeta Tunisia = new CountryMeta(228, "Tunisia", "TN", "TUN", "+216");
        public static readonly CountryMeta Turkey = new CountryMeta(229, "Turkey", "TR", "TUR", "+90");
        public static readonly CountryMeta Turkmenistan = new CountryMeta(230, "Turkmenistan", "TM", "TKM", "+993");
        public static readonly CountryMeta Turks_and_Caicos_Islands = new CountryMeta(231, "Turks and Caicos Islands", "TC", "TCA", "+1649");
        public static readonly CountryMeta Tuvalu = new CountryMeta(232, "Tuvalu", "TV", "TUV", "+688");
        public static readonly CountryMeta Uganda = new CountryMeta(233, "Uganda", "UG", "UGA", "+256");
        public static readonly CountryMeta Ukraine = new CountryMeta(234, "Ukraine", "UA", "UKR", "+380");
        public static readonly CountryMeta United_Arab_Emirates = new CountryMeta(235, "United Arab Emirates", "AE", "ARE", "+971");
        public static readonly CountryMeta United_Kingdom = new CountryMeta(236, "United Kingdom", "GB", "GBR", "+44");
        public static readonly CountryMeta United_States = new CountryMeta(237, "United States", "US", "USA", "+1");
        public static readonly CountryMeta United_States_Minor_Outlying_Islands = new CountryMeta(238, "United States Minor Outlying Islands", "UM", "UMI", "+601");
        public static readonly CountryMeta Uruguay = new CountryMeta(239, "Uruguay", "UY", "URY", "+598");
        public static readonly CountryMeta Uzbekistan = new CountryMeta(240, "Uzbekistan", "UZ", "UZB", "+998");
        public static readonly CountryMeta Vanuatu = new CountryMeta(241, "Vanuatu", "VU", "VUT", "+678");
        public static readonly CountryMeta Venezuela = new CountryMeta(242, "Venezuela", "VE", "VEN", "+58");
        public static readonly CountryMeta Vietnam = new CountryMeta(243, "Vietnam", "VN", "VNM", "+84");
        public static readonly CountryMeta Virgin_Islands_British = new CountryMeta(244, "Virgin Islands, British", "VG", "VGB", "+1284");
        public static readonly CountryMeta Virgin_Islands_US = new CountryMeta(245, "Virgin Islands, U.S.", "VI", "VIR", "+1340");
        public static readonly CountryMeta Wallis_and_Futuna = new CountryMeta(246, "Wallis and Futuna", "WF", "WLF", "+681");
        public static readonly CountryMeta Western_Sahara = new CountryMeta(247, "Western Sahara", "EH", "ESH", "+212");
        public static readonly CountryMeta Yemen = new CountryMeta(248, "Yemen", "YE", "YEM", "+967");
        public static readonly CountryMeta Zambia = new CountryMeta(249, "Zambia", "ZM", "ZMB", "+260");
        public static readonly CountryMeta Zimbabwe = new CountryMeta(250, "Zimbabwe", "ZW", "ZWE", "+263");
    }
}
