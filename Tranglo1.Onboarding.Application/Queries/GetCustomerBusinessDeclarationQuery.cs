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
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.BusinessDeclaration;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementBusinessDeclaration.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]
    public class GetCustomerBusinessDeclarationQuery : IRequest<GetCustomerBusinessDeclarationOutputDTO>
    {
        public int BusinessProfileCode { get; set; }

        public class GetCustomerBusinessDeclarationQueryHandler : IRequestHandler<GetCustomerBusinessDeclarationQuery, GetCustomerBusinessDeclarationOutputDTO>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IConfiguration _config;
            public GetCustomerBusinessDeclarationQueryHandler(IBusinessProfileRepository repository, IConfiguration config)
            {
                _repository = repository;
                _config = config;
            }

            public async Task<GetCustomerBusinessDeclarationOutputDTO> Handle(GetCustomerBusinessDeclarationQuery request, CancellationToken cancellationToken)
            {
                IEnumerable<CustomerBusinessDeclarationAnswerList> customerBusinessDeclarationAnswerResult;

                var outputDTO = new GetCustomerBusinessDeclarationOutputDTO();
                var customerBusinessDeclarationAnswers = new List<CustomerBusinessDeclarationAnswerList>();

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "GetCustomerBusinessDeclaration",
                        new
                        {
                            BusinessProfileCode = request.BusinessProfileCode
                        },
                        null, null, CommandType.StoredProcedure);

                    outputDTO = await reader.ReadFirstAsync<GetCustomerBusinessDeclarationOutputDTO>();
                    customerBusinessDeclarationAnswerResult = await reader.ReadAsync<CustomerBusinessDeclarationAnswerList>();
                    customerBusinessDeclarationAnswers = customerBusinessDeclarationAnswerResult.ToList();

                    outputDTO.CustomerBusinessDeclarationAnswers = customerBusinessDeclarationAnswers;

                    var customerBusinessDeclaration = await _repository.GetCustomerBusinessDeclarationByBusinessProfileCode(request.BusinessProfileCode);
                    outputDTO.IsRedoBusinessDeclaration = customerBusinessDeclaration.IsRedoBusinessDeclaration;

                    return outputDTO;
                }
            }
        }
    }
}
