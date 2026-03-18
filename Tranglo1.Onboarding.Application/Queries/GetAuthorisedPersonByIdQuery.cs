using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.AuthorisedPerson;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementOwnership.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] {  })]
    internal class GetAuthorisedPersonByIdQuery : BaseQuery<IEnumerable<AuthorisedPersonOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<AuthorisedPersonOutputDTO> result)
        {

            string _description = $"Get Authorised Person for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetAuthorisedPersonByIdQueryHandler : IRequestHandler<GetAuthorisedPersonByIdQuery, IEnumerable<AuthorisedPersonOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly IConfiguration _config;

            public GetAuthorisedPersonByIdQueryHandler(IBusinessProfileRepository repository, IConfiguration config, BusinessProfileService businessProfileService)
            {
                this._repository = repository;
                this._config = config;
                this._businessProfileService = businessProfileService;
            }


            public async Task<IEnumerable<AuthorisedPersonOutputDTO>> Handle(GetAuthorisedPersonByIdQuery request, CancellationToken cancellationToken)
            {
                var solution = Solution.Connect;

                if ((request?.CustomerSolution != null && request?.CustomerSolution == ClaimCode.Business) ||
                    (request?.AdminSolution == null && request?.AdminSolution == Solution.Business.Id)){
                    solution = Solution.Business;
                }

                IEnumerable<AuthorisedPersonOutputDTO> outputDTO;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                using(var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var queryParameters = new
                    {
                        BusinessProfileCode = request.BusinessProfileCode
                    };

                  
                    var reader = await connection.QueryMultipleAsync(
                        "dbo.GetAuthorisedPersonByBusinessProfileCode",
                        queryParameters,
                        commandType: CommandType.StoredProcedure);

                    outputDTO = await reader.ReadAsync<AuthorisedPersonOutputDTO>();
                }

                var _isAuthorisedPersonCompleted = await _businessProfileService.IsOwnershipAuthorisedPersonsCompleted(request.BusinessProfileCode, solution);

                for (int i = 0; i < outputDTO.Count(); i++)
                {
                    outputDTO.ElementAt(i).isCompleted = _isAuthorisedPersonCompleted[i];
                }
                return outputDTO;
            }
        }
    }
}