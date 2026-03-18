  
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
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetKYCApprovalDetailsQuery : BaseQuery<Result<KYCApprovalDetailsOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public long? SolutionCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<KYCApprovalDetailsOutputDTO> result)
        {        
            if (result.IsSuccess)
            {
                string _description = $"Viewed partner's KYC details";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class GetKYCApprovalDetailsQueryHandler : IRequestHandler<GetKYCApprovalDetailsQuery, Result<KYCApprovalDetailsOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly IPartnerRepository _partnerRepository;
            private readonly PartnerService _partnerService;

            public GetKYCApprovalDetailsQueryHandler(IConfiguration config, IPartnerRepository partnerRepository, PartnerService partnerService)
            {
                _config = config;
                _partnerRepository = partnerRepository;
                _partnerService = partnerService;
            }


            public async Task<Result<KYCApprovalDetailsOutputDTO>> Handle(GetKYCApprovalDetailsQuery request, CancellationToken cancellationToken)
            {
                var partnerRegistration = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistration.Id);

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var resultList = await connection.QueryAsync<KYCApprovalDetailsOutputDTO>(
                        "GetKYCApprovalDetails",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode,
                            AdminSolution = request.SolutionCode
                        },
                        null, null, CommandType.StoredProcedure);                    

                    if (resultList.Count() == 0)
                    {
                        return Result.Failure<KYCApprovalDetailsOutputDTO>("No Result");
                    }

                    if (bilateralPartnerFlow != null)                    
                    {
                        PartnerType partnerType = bilateralPartnerFlow;
                        resultList.First().PartnerTypeCode = partnerType.Id;
                        resultList.First().PartnerTypeDescription = partnerType.Name;
                    }

                    if (request.SolutionCode.HasValue)
                    {
                        var solution = await _partnerRepository.GetSolutionAsync(request.SolutionCode.Value);
                        resultList.First().SolutionCode = solution.Id;
                        resultList.First().SolutionDescription = solution.Name;
                    }

                    if (request.SolutionCode == Solution.Business.Id)
                    {
                        resultList.First().KYCSubmissionDate = resultList.First().BusinessKYCSubmissionDate;
                    }
                    return Result.Success(resultList.First());
                }
            }
        }
    }
}