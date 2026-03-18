using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetKYCReviewResultQuery : BaseQuery<Result<KYCReviewResultOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public long KYCCategoryCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<KYCReviewResultOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Review Result for Business Profile Code: [{this.BusinessProfileCode}] and Category Code: [{this.KYCCategoryCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class GetKYCReviewResultQueryHandler : IRequestHandler<GetKYCReviewResultQuery, Result<KYCReviewResultOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;

            public GetKYCReviewResultQueryHandler(IBusinessProfileRepository repo, IMapper mapper)
            {
                _repository = repo;
                _mapper = mapper;

            } 

            public async Task<Result<KYCReviewResultOutputDTO>> Handle(GetKYCReviewResultQuery request, CancellationToken cancellationToken)
            {
                var res = await _repository.GetReviewResultByCodeAsync(request.BusinessProfileCode, request.KYCCategoryCode);
                if (res != null)
                {
                    var result = new KYCReviewResultOutputDTO() { ReviewResultCode = res?.Id, ReviewResultDescription = res?.Name };
                    return Result.Success(result);
                }
                return Result.Failure<KYCReviewResultOutputDTO>(
                           $"KYC review result request {request.BusinessProfileCode} not found.");

            }
        }

    }

    

}

