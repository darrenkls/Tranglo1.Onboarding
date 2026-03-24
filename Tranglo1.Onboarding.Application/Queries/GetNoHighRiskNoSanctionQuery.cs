using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Domain.Entities;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetNoHighRiskNoSanctionQuery : IRequest<IReadOnlyList<CountryListOutputDTO>>
    {
        public class GetDisplayedCountriesQueryHandler : IRequestHandler<GetNoHighRiskNoSanctionQuery, IReadOnlyList<CountryListOutputDTO>>
        {
            private readonly ICountrySettingRepository _repository;
            //private readonly ICountryRepository _countryRepository;
            public GetDisplayedCountriesQueryHandler(
                ICountrySettingRepository repository)
            {
                //_countryRepository = countryRepository;
                _repository = repository;
            }

            public async Task<IReadOnlyList<CountryListOutputDTO>> Handle(GetNoHighRiskNoSanctionQuery request, CancellationToken cancellationToken)
            {
                var countriesMeta = await _repository.GetNoHighRiskNoSanctionCountriesAsync();

                //Initialize the mapper
                var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<CountryMeta, CountryListOutputDTO>()
                    .ForMember(o => o.Description, act => act.MapFrom(m => m.Name))
                );

                //Using automapper
                var mapper = new Mapper(config);
                return mapper.Map<IReadOnlyList<CountryListOutputDTO>>(countriesMeta);
            }
        }
    }
}
