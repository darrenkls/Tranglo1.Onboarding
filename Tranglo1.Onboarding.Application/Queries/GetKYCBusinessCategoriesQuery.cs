using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetKYCBusinessCategoriesQuery : IRequest<IEnumerable<KYCBusinessCategoriesOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }

        public class GetKYCBusinessCategoriesQueryHandler : IRequestHandler<GetKYCBusinessCategoriesQuery, IEnumerable<KYCBusinessCategoriesOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly PartnerService _partnerService;
            private readonly IPartnerRepository _partnerRepository;

            public GetKYCBusinessCategoriesQueryHandler(IBusinessProfileRepository repository, PartnerService partnerService, IPartnerRepository partnerRepository)
            {
                _repository = repository;
                _partnerService = partnerService;
                _partnerRepository = partnerRepository;
            }
           

            public async Task<IEnumerable<KYCBusinessCategoriesOutputDTO>> Handle(GetKYCBusinessCategoriesQuery request, CancellationToken cancellationToken)
            {
                var OutputDTO = new List<KYCBusinessCategoriesOutputDTO>();
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerRegistrationInfo.CustomerTypeCode.Value);
                var kycCategoryCustomerTypes = await _partnerRepository.GetKYCBusinessCategoryByCustomerTypeGroupCodeAsync(customerType.CustomerTypeGroupCode);
              

                foreach(var kycCat in kycCategoryCustomerTypes)
                {
                    var kycBusinessCategory = Enumeration.FindById<KYCCategory>(kycCat.KYCCategory.Id);

                    if (kycBusinessCategory != null)
                    {
                        var kycBusinessCategoryOutput = new KYCBusinessCategoriesOutputDTO
                        {
                            KYCCategoryCode = kycBusinessCategory.Id,
                            Description = kycBusinessCategory.Name
                        };
                        OutputDTO.Add(kycBusinessCategoryOutput);
                    }
                }
                return OutputDTO;
            }
        }
    }
}
