using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetDocumentMetaData : BaseQuery<IEnumerable<DocumentMetaDataOutputDTO>>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(IEnumerable<DocumentMetaDataOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Document Meta Data for Document Category Code: [{this.DocumentCategoryCode}] and Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Document Meta Data for Document Category Code: [{this.DocumentCategoryCode}] and Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetDocumentMetaDataHandler : IRequestHandler<GetDocumentMetaData, IEnumerable<DocumentMetaDataOutputDTO>>
    {
        private readonly IConfiguration _config;

        public GetDocumentMetaDataHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<DocumentMetaDataOutputDTO>> Handle(GetDocumentMetaData request, CancellationToken cancellationToken)
        {
            var _connectionString = _config.GetConnectionString("DefaultConnection");

            IEnumerable<DocumentMetaDataOutputDTO> DocumentMetaDataDtos;
        

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetDocumentMetaDataByBusinessProfile&CategoryCode",
                    new
                    {
                        DocumentCategoryCode = request.DocumentCategoryCode,
                        BusinessProfileCode = request.BusinessProfileCode
                    },
                    null, null, CommandType.StoredProcedure);

                // read as IEnumerable<dynamic>
                DocumentMetaDataDtos = await reader.ReadAsync<DocumentMetaDataOutputDTO>();
                
            }
            return DocumentMetaDataDtos;
        }
    }
}
