using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetBusinessProfilePendingKYCRequisitionQuery : BaseQuery<Result<PartnerKYCPendingRequsitionOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public int AdminSolution { get; set; }
        internal class GetBusinessProfilePendingKYCRequisitionQueryHandler : IRequestHandler<GetBusinessProfilePendingKYCRequisitionQuery, Result<PartnerKYCPendingRequsitionOutputDTO>>
        {
            private readonly IConfiguration _config;
            
            public GetBusinessProfilePendingKYCRequisitionQueryHandler(IConfiguration configuration)
            {
                _config = configuration;
            }

            public async Task<Result<PartnerKYCPendingRequsitionOutputDTO>> Handle(GetBusinessProfilePendingKYCRequisitionQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = (await connection.QueryAsync<PartnerKYCPendingRequsitionOutputDTO>("GetPendingPartnerKYCRequisition",
                        new
                        {
                            request.BusinessProfileCode
                        }, commandType: System.Data.CommandType.StoredProcedure)).FirstOrDefault();

                    if(reader == null)
                    {

                        return Result.Success(new PartnerKYCPendingRequsitionOutputDTO
                        {
                            PendingRequisition = false
                        });
                    }
                    else
                    {
                        reader.PendingRequisition = true;
                        return Result.Success(reader);
                    }
                }
                
            }
        }
    }
}
