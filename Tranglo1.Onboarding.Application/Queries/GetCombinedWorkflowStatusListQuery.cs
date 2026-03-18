using AutoMapper;
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
using Tranglo1.Onboarding.Application.DTO.Meta;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetCombinedWorkflowStatusListQuery : IRequest<IEnumerable<WorkflowStatusListOutputDTO>>
    {
        public class GetCombinedWorkflowStatusListQueryHandler : IRequestHandler<GetCombinedWorkflowStatusListQuery, IEnumerable<WorkflowStatusListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            private readonly IConfiguration _config;
            public GetCombinedWorkflowStatusListQueryHandler(IBusinessProfileRepository repo, IMapper mapper, IConfiguration config)
            {
                _repository = repo;
                _mapper = mapper;
                _config = config;
            }


            public async Task<IEnumerable<WorkflowStatusListOutputDTO>> Handle(GetCombinedWorkflowStatusListQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");


                IEnumerable<WorkflowStatusListOutputDTO> combinedWorkflowStatusListOutputDtos;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetCombinedWorkflowStatus",
                        CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    combinedWorkflowStatusListOutputDtos = await reader.ReadAsync<WorkflowStatusListOutputDTO>();


                }
                return combinedWorkflowStatusListOutputDtos;
            }
        }
    }
}
