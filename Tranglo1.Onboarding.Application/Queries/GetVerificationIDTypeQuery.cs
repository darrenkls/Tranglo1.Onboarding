using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetVerificationIDTypeQuery : IRequest<IEnumerable<VerificationIDTypeOutputDTO>>
    {
        public class GetVerificationIDTypeQueryHandler : IRequestHandler<GetVerificationIDTypeQuery, IEnumerable<VerificationIDTypeOutputDTO>>
        {
            private readonly ApplicationUserDbContext _context;
            private readonly IConfiguration _config;
            private readonly IBusinessProfileRepository _repository;

            public GetVerificationIDTypeQueryHandler(ApplicationUserDbContext context, IConfiguration config, IBusinessProfileRepository repository)
            {
                _context = context;
                _config = config;
                _repository = repository;
            }

            public async Task<IEnumerable<VerificationIDTypeOutputDTO>> Handle(GetVerificationIDTypeQuery request, CancellationToken cancellationToken)
            {

                var verificationIDType = await _repository.GetAllVerificationIDTypes();

                IEnumerable<VerificationIDTypeOutputDTO> outputDTO = verificationIDType.Select(a => new VerificationIDTypeOutputDTO
                    {
                    VerificationIDTypeCode = a.Id,
                    VerificationIDTypeDescription = a.Name
                    });

                return outputDTO;


            }
        }
    }
}
