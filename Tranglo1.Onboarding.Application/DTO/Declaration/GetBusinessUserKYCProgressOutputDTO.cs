using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;

namespace Tranglo1.Onboarding.Application.DTO.Declaration
{
    public class GetBusinessUserKYCProgressOutputDTO
    {
        [JsonIgnore]
        public KYCCategory KYCCategory { get; set; }
        public long? KYCCategoryCode
        {
            get
            {
                return KYCCategory?.Id;
            }
        }
        public string KYCCategoryDescription
        {
            get
            {
                return KYCCategory.Name;
            }
        }

        [JsonIgnore]
        public KYCProgressStatus KYCProgressStatus { get; set; }
        public int Status
        {
            get
            {
                return (int?)KYCProgressStatus?.Id ?? 0;
            }
        }
        public string StatusDesc
        {
            get
            {
                return KYCProgressStatus?.Name ?? string.Empty;
            }
        }

        public List<GetBusinessUserKYCProgressSubMenuOutputDTO> BusinessUserKYCSubItems { get; set; }
    }

    public class GetBusinessUserKYCProgressSubMenuOutputDTO
    {
        [JsonIgnore]
        public KYCSubCategory KYCSubCategory { get; set; }

        public string KYCCategoryDescription
        {
            get
            {
                return KYCSubCategory.Name;
            }
        }

        [JsonIgnore]
        public KYCProgressStatus KYCProgressStatus { get; set; }
        public int Status
        {
            get
            {
                return (int?)KYCProgressStatus?.Id ?? 0;
            }
        }
        public string StatusDesc
        {
            get
            {
                return KYCProgressStatus?.Name ?? string.Empty;
            }
        }
    }
}
