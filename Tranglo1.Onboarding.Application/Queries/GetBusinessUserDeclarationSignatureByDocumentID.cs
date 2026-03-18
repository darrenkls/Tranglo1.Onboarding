using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetBusinessUserDeclarationSignatureByDocumentID : BaseQuery<Result<BusinessUserDeclarationSignatureDocumentOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<BusinessUserDeclarationSignatureDocumentOutputDTO> result)
        {
            string _description = $"Get Business User Declarations Signature for Document ID: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetBusinessUserDeclarationSignatureByDocumentIDHandler : IRequestHandler<GetBusinessUserDeclarationSignatureByDocumentID, Result<BusinessUserDeclarationSignatureDocumentOutputDTO>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<GetBusinessUserDeclarationSignatureByDocumentIDHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetBusinessUserDeclarationSignatureByDocumentIDHandler(BusinessProfileService businessProfileService,
                                            IBusinessProfileRepository repository,
                                             ILogger<GetBusinessUserDeclarationSignatureByDocumentIDHandler> logger,
                                             StorageManager storageManager)
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<Result<BusinessUserDeclarationSignatureDocumentOutputDTO>> Handle(GetBusinessUserDeclarationSignatureByDocumentID request, CancellationToken cancellationToken)
        {
            var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);

            if (businessProfile is null)
            {
                return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>(
                           $"Business Profile {request.BusinessProfileCode} doesn't exist."
                       );
            }

            Result<BusinessUserDeclarationSignatureDocumentOutputDTO> result = null;


            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>(
                        $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                    );
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    result = await GetBusinessUserDeclarationSignature(request, businessProfile);

                    if (result.IsFailure)
                    {
                        return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>(
                            $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>(
                        $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                    );
                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    result = await GetBusinessUserDeclarationSignature(request, businessProfile);

                    if (result.IsFailure)
                    {
                        return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>(
                            $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else
                {
                    return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>(
                        $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                    );
                }

                return result;
            }
            else
            {
                return Result.Failure<BusinessUserDeclarationSignatureDocumentOutputDTO>("Invalid request");
            }


        }

        private async Task<Result<BusinessUserDeclarationSignatureDocumentOutputDTO>> GetBusinessUserDeclarationSignature(GetBusinessUserDeclarationSignatureByDocumentID request, BusinessProfile businessProfile)
        {
            var businessUserDeclaration = await _repository.GetBusinessUserDeclarationByBusinessProfileCodeAsync(businessProfile.Id);

            var businessDeclaration = await _repository.GetBusinessUserDeclarationByBusinessProfileCodeAsync(request.BusinessProfileCode);
            if (businessDeclaration == null)
            {
                return null;
            }
            if (businessDeclaration.DocumentId == request.DocumentId)
            {
                var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

                if (document != null)
                {
                    var ms = new MemoryStream();
                    await _storageManager.CopyToAsync(document.DocumentId, ms);
                    ms.Position = 0;
                    return new BusinessUserDeclarationSignatureDocumentOutputDTO()
                    {
                        File = ms,
                        ContentType = document.ContentType,
                        FileName = document.FileName
                    };
                }
            }
            return null;
        }

    }
}
