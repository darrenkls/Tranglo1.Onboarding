using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetBusinessUserDeclarationByBusinessProfileCodeQuery : BaseQuery<Result<GetBusinessUserDeclarationOutputDTO>>
    {
        public long? BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<GetBusinessUserDeclarationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get Business User Declarations for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

    }

    internal class GetBusinessUserDeclarationByBusinessProfileCodeQueryHandler : IRequestHandler<GetBusinessUserDeclarationByBusinessProfileCodeQuery, Result<GetBusinessUserDeclarationOutputDTO>>
    {

        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<GetBusinessUserDeclarationByBusinessProfileCodeQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetBusinessUserDeclarationByBusinessProfileCodeQueryHandler(
            BusinessProfileService businessProfileService,
            IBusinessProfileRepository repository,
            ILogger<GetBusinessUserDeclarationByBusinessProfileCodeQueryHandler>logger,
            StorageManager storageManager)
        {
            _businessProfileService = businessProfileService;
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;

        }
        public async Task<Result<GetBusinessUserDeclarationOutputDTO>> Handle(GetBusinessUserDeclarationByBusinessProfileCodeQuery request, CancellationToken cancellationToken)
        {
            var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);

            if (businessProfile is null)
            {
                return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                           $"Business Profile {request.BusinessProfileCode} doesn't exist."
                       );
            }
            var businessUserDeclaration = await _repository.GetBusinessUserDeclarationByBusinessProfileCodeAsync(businessProfile.Id);



            Result<GetBusinessUserDeclarationOutputDTO> result = null;


            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                        $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                    );
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    result = await GetBusinessUserDeclaration(request, businessProfile, businessUserDeclaration);

                    if (result.IsFailure)
                    {
                        return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                            $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                        $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                    );
                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    result = await GetBusinessUserDeclaration(request, businessProfile, businessUserDeclaration);

                    if (result.IsFailure)
                    {
                        return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                            $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else
                {
                    return Result.Failure<GetBusinessUserDeclarationOutputDTO>(
                        $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                    );
                }

                return result;
            }
            else
            {
                return Result.Failure<GetBusinessUserDeclarationOutputDTO>("Invalid request");
            }     
        }

        private async Task<Result<GetBusinessUserDeclarationOutputDTO>> GetBusinessUserDeclaration(GetBusinessUserDeclarationByBusinessProfileCodeQuery request, BusinessProfile businessProfile, BusinessUserDeclaration businessUserDeclaration)
        {
            var businessSubmissionStatus = await _repository.GetBusinessKYCSubmissionStatusBySubmissionStatusCode(businessProfile.BusinessKYCSubmissionStatus?.Id);
            if(businessSubmissionStatus is null)
            {
                return Result.Failure<GetBusinessUserDeclarationOutputDTO>("Business KYC Submission Status is invalid");
            }
            if (businessUserDeclaration != null)
            {
                GetBusinessUserDeclarationOutputDTO outputDTO = new GetBusinessUserDeclarationOutputDTO
                {
                    BusinessProfileCode = businessUserDeclaration.BusinessProfileCode,
                    BusinessUserDeclarationCode = businessUserDeclaration.Id,
                    isNotRemittancePartner = businessUserDeclaration.IsNotRemittancePartner,
                    isAuthorized = businessUserDeclaration.IsAuthorized,
                    isInformationTrue = businessUserDeclaration.IsInformationTrue,
                    isAgreeTermsOfService = businessUserDeclaration.IsAgreedTermsOfService,
                    isDeclareTransactionTax = businessUserDeclaration.IsDeclareTransactionTax,
                    IsAllApplicationAccurate = businessUserDeclaration.IsAllApplicationAccurate,
                    SigneeName = businessUserDeclaration.SigneeName,
                    Designation = businessUserDeclaration.Designation,
                    DocumentId = businessUserDeclaration?.DocumentId,
                    BusinessKYCSubmissionStatusCode = businessSubmissionStatus?.Id ?? null,
                    BusinessKYCSubmissionStatusDescription = businessSubmissionStatus.Name ?? null,
                    ReviewConcurrentLastModified = businessProfile.ReviewConcurrentLastModified,
                    ReviewConcurrencyToken = businessProfile.ReviewConcurrencyToken,
                    BusinessUserDeclarationConcurrencyToken = businessUserDeclaration.BusinessUserDeclarationConcurrencyToken


                };

                if (businessUserDeclaration.DocumentId.HasValue)
                {
                    var document = await _storageManager.GetDocumentMetadataAsync(businessUserDeclaration.DocumentId.Value);
                    if (document != null)
                    {
                        outputDTO.FileName = document.FileName;
                    }
                }
                return Result.Success<GetBusinessUserDeclarationOutputDTO>(outputDTO);
            }
            else
            {
                // Create and return a default GetBusinessUserDeclarationOutputDTO with BusinessProfileCode set and other properties null
                var defaultOutputDTO = new GetBusinessUserDeclarationOutputDTO
                {
                    BusinessProfileCode = businessProfile.Id,
                    BusinessUserDeclarationCode = null,
                    isNotRemittancePartner = null,
                    isAuthorized = null,
                    isInformationTrue = null,
                    isAgreeTermsOfService = null,
                    isDeclareTransactionTax = null,
                    IsAllApplicationAccurate = null,
                    SigneeName = null,
                    Designation = null,
                    DocumentId = null,
                    FileName = null,
                    BusinessKYCSubmissionStatusCode = businessSubmissionStatus?.Id ?? null,
                    BusinessKYCSubmissionStatusDescription = businessSubmissionStatus.Name ?? null,
                    BusinessUserDeclarationConcurrencyToken = businessUserDeclaration?.BusinessUserDeclarationConcurrencyToken ?? null
                };
                return Result.Success<GetBusinessUserDeclarationOutputDTO>(defaultOutputDTO);
            }

        }
        

    }
}
