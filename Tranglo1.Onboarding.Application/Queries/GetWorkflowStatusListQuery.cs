using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.Meta;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
	public class GetWorkflowStatusListQuery : IRequest<IEnumerable<WorkflowStatusListOutputDTO>>
	{
        public long? AdminSolution { get; set; }
        public class GetWorkflowStatusListQueryHandler : IRequestHandler<GetWorkflowStatusListQuery, IEnumerable<WorkflowStatusListOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly IMapper _mapper;
            private readonly IConfiguration _config;
            public GetWorkflowStatusListQueryHandler(IBusinessProfileRepository repo, IMapper mapper, IConfiguration config)
            {
                _repository = repo;
                _mapper = mapper;
                _config = config;
            }

            public async Task<IEnumerable<WorkflowStatusListOutputDTO>> Handle(GetWorkflowStatusListQuery query, CancellationToken cancellationToken)
            {
                //Specification<WorkflowStatus> spec = Specification<WorkflowStatus>.All;
                //return _mapper.Map<IEnumerable<WorkflowStatus>, IEnumerable<WorkflowStatusListOutputDTO>>(await _repository.GetWorkflowStatusesAsync(spec));

                var _connectionString = _config.GetConnectionString("DefaultConnection");


                IEnumerable<WorkflowStatusListOutputDTO> workflowStatusListOutputDtos;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetComplianceWorkflowStatus",
                        new
                        {
                            AdminSolution = query.AdminSolution
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    workflowStatusListOutputDtos = await reader.ReadAsync<WorkflowStatusListOutputDTO>();


                }
                return workflowStatusListOutputDtos;

            }
        }
    }
}
