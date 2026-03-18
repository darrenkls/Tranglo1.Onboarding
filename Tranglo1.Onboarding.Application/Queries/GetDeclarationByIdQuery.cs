using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.UserAccessControl;
using Microsoft.Extensions.Logging;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Tranglo1.DocumentStorage;
using Tranglo1.Onboarding.Application.DTO.Declarations;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDeclaration, UACAction.View)]
    [Permission(Permission.KYCManagementDeclaration.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { })]
    internal class GetDeclarationByIdQuery : BaseQuery<Result<DeclarationsOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<DeclarationsOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get Declarations for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetDeclarationByIdQueryHandler : IRequestHandler<GetDeclarationByIdQuery, Result<DeclarationsOutputDTO>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<GetDeclarationByIdQuery> _logger;
        private readonly StorageManager _storageManager;
        private readonly IMapper _mapper;

        public GetDeclarationByIdQueryHandler(BusinessProfileService businessProfileService,
                                            IBusinessProfileRepository repository,
                                             ILogger<GetDeclarationByIdQuery> logger,
                                             StorageManager storageManager, IMapper mapper)
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
            _mapper = mapper;
        }

        public async Task<Result<DeclarationsOutputDTO>> Handle(GetDeclarationByIdQuery request, CancellationToken cancellationToken)
        {
            var details = await _businessProfileService.GetKYCDeclarationInfoAsync(request.BusinessProfileCode);

            if (details != null)
            {
                // Load the associated BusinessProfile entity
                var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);

                if (businessProfile != null)
                {
                    var result = _mapper.Map<DeclarationsOutputDTO>(details);

                    if (result != null && (result.DocumentId != null || result.DocumentId != Guid.Empty))
                    {
                        var document = await _storageManager.GetDocumentMetadataAsync(result.DocumentId);
                        if (document != null)
                        {
                            result.FileName = document.FileName;
                        }
                    }

                    // Populate the ConcurrentLastModified property from the BusinessProfile entity
                    result.ReviewConcurrentLastModified = businessProfile.ReviewConcurrentLastModified;
                    result.ReviewConcurrencyToken = businessProfile.ReviewConcurrencyToken;

                    return Result.Success(result);
                }
            }

            return Result.Failure<DeclarationsOutputDTO>(
                               $"KYC declaration for request {request.BusinessProfileCode} not found.");
        }

    }

}
