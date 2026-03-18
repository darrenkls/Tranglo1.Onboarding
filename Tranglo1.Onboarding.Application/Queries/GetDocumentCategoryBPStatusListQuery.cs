using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    public class GetDocumentCategoryBPStatusListQuery : IRequest<IEnumerable<DocumentCategoryBPStatusListOutputDTO>>
    {
        public class GetDocumentCategoryBPStatusListQueryHandler : IRequestHandler<GetDocumentCategoryBPStatusListQuery, IEnumerable<DocumentCategoryBPStatusListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly BusinessProfileDbContext _context;
            private readonly IMapper _mapper;
            public GetDocumentCategoryBPStatusListQueryHandler(BusinessProfileDbContext context, IMapper mapper, IBusinessProfileRepository repository)
            {
                _repository = repository;
                _context = context;
                _mapper = mapper; 
            }
            public async Task<IEnumerable<DocumentCategoryBPStatusListOutputDTO>> Handle(GetDocumentCategoryBPStatusListQuery query, CancellationToken cancellationToken)
            {
                Specification<DocumentCategoryBPStatus> documentCategorySpec = Specification<DocumentCategoryBPStatus>.All;

                var documentCategoryBPStatuses = await _repository.GetDocumentCategoryBPStatusesAsync(documentCategorySpec);

                return _mapper.Map<IEnumerable<DocumentCategoryBPStatus>, IEnumerable<DocumentCategoryBPStatusListOutputDTO>>(documentCategoryBPStatuses);

                
            }
        }
    }
}
