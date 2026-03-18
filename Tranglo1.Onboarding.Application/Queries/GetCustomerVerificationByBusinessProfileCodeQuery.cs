using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementVerification.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] {  })]
    internal class GetCustomerVerificationByBusinessProfileCodeQuery : IRequest<GetCustomerVerificationOutputDTO>
    {
        public long? BusinessProfileCode { get; set; }

        public class GetCustomerVerificationByBusinessProfileCodeQueryHandler : IRequestHandler<GetCustomerVerificationByBusinessProfileCodeQuery, GetCustomerVerificationOutputDTO>
        {
            private readonly IConfiguration _config;
            private readonly IBusinessProfileRepository _repository;

            public GetCustomerVerificationByBusinessProfileCodeQueryHandler(IConfiguration config, IBusinessProfileRepository repository)
            {
                _config = config;
                _repository = repository;

            }

            public async Task<GetCustomerVerificationOutputDTO> Handle(GetCustomerVerificationByBusinessProfileCodeQuery request, CancellationToken cancellationToken)
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    return await GetCustomerVerificationAsync(connection, request.BusinessProfileCode);
                }
            }

            private async Task<GetCustomerVerificationOutputDTO> GetCustomerVerificationAsync(SqlConnection connection, long? businessProfileCode)
            {
                var query = "GetCustomerVerificationByBusinessProfileCode";
                var parameters = new { BusinessProfileCode = businessProfileCode };

                GetCustomerVerificationOutputDTO customerVerificationOutputDTO;
                IEnumerable<GetCustomerVerificationDocumentOutputDTO> customerVerificationDocumentOutputDTOs;

                using (var reader = await connection.QueryMultipleAsync(query, parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    customerVerificationOutputDTO = await reader.ReadSingleOrDefaultAsync<GetCustomerVerificationOutputDTO>();
                    customerVerificationDocumentOutputDTOs = await reader.ReadAsync<GetCustomerVerificationDocumentOutputDTO>();
                }

                if (customerVerificationOutputDTO != null)
                {
                    customerVerificationOutputDTO.GetCustomerVerificationDocuments = customerVerificationDocumentOutputDTOs
                        .Where(x => x.CustomerVerificationCode == customerVerificationOutputDTO.CustomerVerificationCode)
                        .OrderBy(x => x.CustomerVerificationCode)
                        .ToList();
                }

                var customerVerification = await _repository.GetCustomerVerificationbyBusinessProfileCodeAsync((int)businessProfileCode);
                customerVerificationOutputDTO.CustomerVerificationConcurrencyToken = customerVerification?.CustomerVerificationConcurrencyToken;

                return customerVerificationOutputDTO;
            }
        }
    }
}
