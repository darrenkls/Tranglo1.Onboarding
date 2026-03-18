using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetPartnerRegistrationLeadsOriginsQuery : IRequest<IEnumerable<PartnerRegistrationLeadsOriginOutputDTO>>
    {
        public class GetPartnerRegistrationLeadsOriginsQueryHandler : IRequestHandler<GetPartnerRegistrationLeadsOriginsQuery, IEnumerable<PartnerRegistrationLeadsOriginOutputDTO>>
        {
            public Task<IEnumerable<PartnerRegistrationLeadsOriginOutputDTO>> Handle(GetPartnerRegistrationLeadsOriginsQuery request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Enumeration.GetAll<PartnerRegistrationLeadsOrigin>()
                    .Select(x => new PartnerRegistrationLeadsOriginOutputDTO
                    {
                        PartnerRegistrationLeadsOriginCode = x.Id,
                        Description = x.Name
                    })
                    .OrderBy(x => x.PartnerRegistrationLeadsOriginCode)
                    .AsEnumerable());
            }
        }
    }
}
