using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerDetails, UACAction.View)]
    [Permission(Permission.ManagePartnerPartnerDetails.Action_View_Code,
       new int[] { (int)PortalCode.Admin },
       new string[] {  })]
    class GetIndividualPartnerListingQuery : BaseQuery<Result<IndividualPartnerListingOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public string TrangloEntity { get; set; }

        public class GetIndividualPartnerListingQueryHandler : IRequestHandler<GetIndividualPartnerListingQuery, Result<IndividualPartnerListingOutputDTO>>
        {
            private readonly IMapper _mapper;
            private readonly IConfiguration _config;
            private IHttpClientFactory _HttpClientFactory;
            
            public GetIndividualPartnerListingQueryHandler(IMapper mapper, IConfiguration config, IHttpClientFactory httpClientFactory)
            {
                _mapper = mapper;
                _config = config;
                _HttpClientFactory = httpClientFactory;
            }

            public async Task<Result<IndividualPartnerListingOutputDTO>> Handle(GetIndividualPartnerListingQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                IndividualPartnerListingOutputDTO individualPartnerListingOutputDTOs;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    individualPartnerListingOutputDTOs = await connection.QueryFirstOrDefaultAsync<IndividualPartnerListingOutputDTO>(
                        "dbo.GetIndividualPartnerListing",
                        new
                        {
                            PartnerCode = request.PartnerCode,
                            TrangloEntity = request.TrangloEntity
                        },
                        null, null,
                         CommandType.StoredProcedure);
                }

                return Result.Success(individualPartnerListingOutputDTOs);
            }
        }
    }
}
