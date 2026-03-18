using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.PoliticallyExposedPerson;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.View)]
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetPoliticallyExposedPersonByIdQuery : BaseQuery<IEnumerable<PoliticallyExposedPersonOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<PoliticallyExposedPersonOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Politically Exposed Persons (PEP) for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Politically Exposed Persons (PEP) for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetPoliticallyExposedPersonByIdQueryHandler : IRequestHandler<GetPoliticallyExposedPersonByIdQuery, IEnumerable<PoliticallyExposedPersonOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly BusinessProfileService _businessProfileService;

            public GetPoliticallyExposedPersonByIdQueryHandler(IMapper mapper, BusinessProfileService businessProfileService)
            {
                _mapper = mapper;
                _businessProfileService = businessProfileService;
            }

            public async Task<IEnumerable<PoliticallyExposedPersonOutputDTO>> Handle(GetPoliticallyExposedPersonByIdQuery query, CancellationToken cancellationToken)
            {
                var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);

                var person = await _businessProfileService.GetPoliticallyExposedPersonByBusinessProfileCodeAsync(businessProfile.Value);

                var politicallyExposedPersonsOutput = _mapper.Map<IEnumerable<PoliticallyExposedPerson>, IEnumerable<PoliticallyExposedPersonOutputDTO>>(person.Value);

                foreach (var pepOutput in politicallyExposedPersonsOutput)
                {
                    // Set the concurrency token from BusinessProfile
                    pepOutput.PoliticalExposedPersonsConcurrencyToken = businessProfile.Value.PoliticalExposedPersonsConcurrencyToken;
                }

                return politicallyExposedPersonsOutput;
            }
        }
    }
}
