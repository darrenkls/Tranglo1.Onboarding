using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetCustomerRegisteredCountryByBusinessProfileCodeQuery : BaseQuery<Result<CustomerUserRegisteredCountryOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public override Task<string> GetAuditLogAsync(Result<CustomerUserRegisteredCountryOutputDTO> result)
        {
            string _description = $"Get Partner Tranglo Entity  for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetCustomerRegisteredCountryByBusinessProfileCodeQueryHandler : IRequestHandler<GetCustomerRegisteredCountryByBusinessProfileCodeQuery, Result<CustomerUserRegisteredCountryOutputDTO>>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ILogger<GetCustomerRegisteredCountryByBusinessProfileCodeQueryHandler> _logger;

        public GetCustomerRegisteredCountryByBusinessProfileCodeQueryHandler(IBusinessProfileRepository businessProfileRepository, 
            ILogger<GetCustomerRegisteredCountryByBusinessProfileCodeQueryHandler> logger)
        {
            _businessProfileRepository = businessProfileRepository;
            _logger = logger;
        }

        public async Task<Result<CustomerUserRegisteredCountryOutputDTO>> Handle(GetCustomerRegisteredCountryByBusinessProfileCodeQuery request, CancellationToken cancellationToken)
        {
            // Retrieve business profile data by profile code
            var customerUserBusinessProfile = await _businessProfileRepository.GetCustomerUserBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);

            if (customerUserBusinessProfile == null)
            {
                return Result.Failure<CustomerUserRegisteredCountryOutputDTO>($"Business profile not found.");
            }

            var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
            if (businessProfile == null)
            {
                return Result.Failure<CustomerUserRegisteredCountryOutputDTO>($"Business profile not found.");
            }
            // Retrieve country metadata using ISO2 code
            var customerUserCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(customerUserBusinessProfile.CustomerUser.CountryMeta.CountryISO2);

            if (customerUserCountry == null)
            {
                return Result.Failure<CustomerUserRegisteredCountryOutputDTO>($"Country Not Found.");
            }

            var companyRegisteredCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(businessProfile.CompanyRegisteredCountryMeta.CountryISO2);
            if (companyRegisteredCountry == null)
            {
                return Result.Failure<CustomerUserRegisteredCountryOutputDTO>($"Registered Country Not Found.");
            }
            
            // Construct the output DTO
            var outputDto = new CustomerUserRegisteredCountryOutputDTO
            {
                CustomerUserBusinessProfileCode = customerUserBusinessProfile.BusinessProfileCode,
                CustomerUserId = customerUserBusinessProfile.CustomerUser.Id,
                CountryCode = customerUserBusinessProfile.CustomerUser.CountryMeta.Id,  
                CountryISO2 = customerUserCountry.CountryISO2,
                CountryDescription = customerUserCountry.Name,
                CompanyRegisteredCountryCode = companyRegisteredCountry.Id,
                CompanyRegisteredCountryISO2 = companyRegisteredCountry.CountryISO2,
                CompanyRegisteredCountryDescription = companyRegisteredCountry.Name,
            };

            if (companyRegisteredCountry.CountryISO2 == CountryMeta.Malaysia.CountryISO2)
            {
                outputDto.IsTINMandatory = true;
            }

            return outputDto;
        }



    }
}



