using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetCustomerVerificationRiskResultsByVerificationCodeQuery : BaseQuery<Result<GetCustomerVerificationRiskResultsOutputDTO>>
    {
        public long? CustomerVerificationCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<GetCustomerVerificationRiskResultsOutputDTO> result)
        {
            string _description = $"Get Risk Details for Customer Verification Code: [{this.CustomerVerificationCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetCustomerVerificationRiskResultsByVerificationCodeQueryHandler : IRequestHandler<GetCustomerVerificationRiskResultsByVerificationCodeQuery, Result<GetCustomerVerificationRiskResultsOutputDTO>>
    {
        public IBusinessProfileRepository _repository;
        public ILogger<GetCustomerVerificationRiskResultsByVerificationCodeQueryHandler> _logger;

        public GetCustomerVerificationRiskResultsByVerificationCodeQueryHandler(IBusinessProfileRepository repository, ILogger<GetCustomerVerificationRiskResultsByVerificationCodeQueryHandler> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<Result<GetCustomerVerificationRiskResultsOutputDTO>> Handle(GetCustomerVerificationRiskResultsByVerificationCodeQuery request, CancellationToken cancellationToken)
        {
            var customerVerification = await _repository.GetCustomerVerificationbyCustomerVerificationCodeAsync(request.CustomerVerificationCode);

            if (customerVerification == null)
            {
                return Result.Failure<GetCustomerVerificationRiskResultsOutputDTO>($"Customer Vericication Code {request.CustomerVerificationCode} doest not exist.");
            }

            var outputDTO = new GetCustomerVerificationRiskResultsOutputDTO
            {
                CustomerVerificationCode = customerVerification?.Id,
                SubmissionDate = customerVerification.SubmissionDate,
                SubmissionCount = customerVerification.SubmissionCount
            };

            return outputDTO;
        }
    }
}
