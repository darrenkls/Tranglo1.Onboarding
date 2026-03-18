using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.Meta;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class SubmitVerificationCommand : BaseCommand<Result<SubmitVerificationOutputDTO>>
    {
        public long CustomerVerificationCode { get; set; }
    }

    internal class SubmitVerificationCommandHandler : IRequestHandler<SubmitVerificationCommand, Result<SubmitVerificationOutputDTO>>
    {
        private readonly IConfiguration _config;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IBusinessProfileRepository _repository;

        public SubmitVerificationCommandHandler(IConfiguration config, BusinessProfileService businessProfileService, IBusinessProfileRepository repository)
        {
            _config = config;
            _businessProfileService = businessProfileService;
            _repository = repository;
        }

        public async Task<Result<SubmitVerificationOutputDTO>> Handle(SubmitVerificationCommand request, CancellationToken cancellationToken) 
        {
            var outputDTO = new SubmitVerificationOutputDTO();

            var customerVerification = await _repository.GetCustomerVerificationbyCustomerVerificationCodeAsync(request.CustomerVerificationCode);
            if (customerVerification is null)
            {
                return Result.Failure<SubmitVerificationOutputDTO>(
                                $"Unable to submit for verification. CustomerVerification {request.CustomerVerificationCode} doesn't exist"
                                );
            }
            else
            {
                // 1) Insert into CDC table -> cdc.JumioVerifications


                // 2) Update SubmissionResult to 'Processing'
                var customerVerificationDocuments = await _repository.GetCustomerVerificationDocumentbyCustomerVerificationCodeAsync(customerVerification.Id);
                if (customerVerificationDocuments is null)
                {
                    return Result.Failure<SubmitVerificationOutputDTO>(
                                    $"Unable to submit for verification. CustomerVerificationDocuments doesn't exist for CustomerVerificationCode: {request.CustomerVerificationCode}"
                                    );
                }
                else
                {
                    var processing = await _repository.GetSubmissionResultByCode(SubmissionResult.Processing.Id);
                    var customerVerificationDocumentOutputDTOs = new List<CustomerVerificationDocumentList>();

                    foreach (var c in customerVerificationDocuments)
                    {
                        var customerVerificationDocumentOutputDTO = new CustomerVerificationDocumentList();
                        c.SubmissionResult = processing;

                        customerVerificationDocumentOutputDTO.CustomerVerificationDocumentCode = c.Id;
                        customerVerificationDocumentOutputDTO.SubmissionResultCode = processing.Id;
                        customerVerificationDocumentOutputDTO.SubmissionResultDescription = processing.Name;
                        customerVerificationDocumentOutputDTOs.Add(customerVerificationDocumentOutputDTO);
                    }

                    var update = await _repository.UpdateCustomerVerificationDocumentsListAsync(customerVerificationDocuments);

                    // 3) Prepare outputDTO
                    if (update.IsFailure)
                    {
                        return Result.Failure<SubmitVerificationOutputDTO>(
                                        $"Unable to update Submission Results for CustomerVerificationCode: {request.CustomerVerificationCode}"
                                        );
                    }
                    else
                    {
                        var eKYCVerificationStatus = await _repository.GetVerificationStatusByCodeAsync(customerVerification.EKYCVerificationStatus?.Id);
                        var f2FVerificationStatus = await _repository.GetVerificationStatusByCodeAsync(customerVerification.F2FVerificationStatus?.Id);

                        outputDTO.CustomerVerificationCode = customerVerification.Id;
                        outputDTO.EKYCVerificationStatusCode = eKYCVerificationStatus?.Id;
                        outputDTO.EKYCVerificationStatusDescription = eKYCVerificationStatus?.Name;
                        outputDTO.F2FVerificationStatusCode = f2FVerificationStatus?.Id;
                        outputDTO.F2FVerificationStatusDescription = f2FVerificationStatus?.Name;
                        outputDTO.CustomerVerificationDocuments = customerVerificationDocumentOutputDTOs;
                    }
                }
            }                               

            return Result.Success(outputDTO);
        }
    }
}