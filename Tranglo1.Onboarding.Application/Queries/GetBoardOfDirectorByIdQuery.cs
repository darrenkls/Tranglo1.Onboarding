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
using Tranglo1.Onboarding.Application.DTO.BoardofDirector;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.View)]
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetBoardOfDirectorByIdQuery : BaseQuery<IEnumerable<BoardofDirectorOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<BoardofDirectorOutputDTO> result)
        {

            string _description = $"Get Board of Directors for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetBoardOfDirectorByIdQueryHandler : IRequestHandler<GetBoardOfDirectorByIdQuery, IEnumerable<BoardofDirectorOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly BusinessProfileService _businessProfileService;

            public GetBoardOfDirectorByIdQueryHandler(IMapper mapper, BusinessProfileService businessProfileService)
            {
                _mapper = mapper;
                _businessProfileService = businessProfileService;
            }

            public async Task<IEnumerable<BoardofDirectorOutputDTO>> Handle(GetBoardOfDirectorByIdQuery query, CancellationToken cancellationToken)
            {

                var businessProfile = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);

                var BOD = await _businessProfileService.GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile.Value);

                var _isBoardOfDirectorCompleted = await _businessProfileService.IsOwnershipBoardOfDirectorsCompleted(businessProfile.Value.Id);

                var boardOfDirectorDTO = _mapper.Map<IEnumerable<BoardOfDirector>, IEnumerable<BoardofDirectorOutputDTO>>(BOD.Value);

                for (int i = 0; i < boardOfDirectorDTO.Count(); i++)
                {
                    boardOfDirectorDTO.ElementAt(i).isCompleted = _isBoardOfDirectorCompleted[i];
                }


                foreach (var bodOutput in boardOfDirectorDTO)
                {
                    // Set the concurrency token from BusinessProfile
                    bodOutput.BoardOfDirectorConcurrencyToken = businessProfile.Value.BoardOfDirectorConcurrencyToken;
                }

                return boardOfDirectorDTO;
            }
        }
    }
}
