using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetAllPartnerSubscriptionQuery : BaseQuery<Result<GetAllPartnerSubscriptionOutputDTO>>
    {
        public long PartnerCode { get; set; }
       

        public class GetAllPartnerSubscriptionQueryHandler : IRequestHandler<GetAllPartnerSubscriptionQuery, Result<GetAllPartnerSubscriptionOutputDTO>>
        {
            private readonly IPartnerRepository _partnerRepository;
            private readonly IConfiguration _config;
            private readonly ILogger<GetAllPartnerSubscriptionQueryHandler> _logger;

            public GetAllPartnerSubscriptionQueryHandler(IPartnerRepository partnerRepository, IConfiguration config, ILogger<GetAllPartnerSubscriptionQueryHandler> logger)
            {
                _partnerRepository = partnerRepository;
                _config = config;
                _logger = logger;
            }

            public async Task<Result<GetAllPartnerSubscriptionOutputDTO>> Handle(GetAllPartnerSubscriptionQuery request, CancellationToken cancellationTokenGetAllPartnerSubscriptionQuery)
            {
                var checkSubscriptionList = await _partnerRepository.GetPartnerSubscriptionListAsync(request.PartnerCode);

                if (checkSubscriptionList.Count == 0)
                {
                    return Result.Failure<GetAllPartnerSubscriptionOutputDTO>($"No subscription exist for partner {request.PartnerCode}");
                }

                var _connectionString = _config.GetConnectionString("DefaultConnection");

              
                IEnumerable<PartnerSubscriptionList> partnerSubscriptionListResult;

                GetAllPartnerSubscriptionOutputDTO outputDto = new GetAllPartnerSubscriptionOutputDTO();
                List<PartnerSubscriptionList> subscriptions = new List<PartnerSubscriptionList>();

                try
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetPartnerAllSubscription",
                           new
                           {
                               PartnerCode = request.PartnerCode,
                             
                           },
                           null, null, CommandType.StoredProcedure);

                
                        partnerSubscriptionListResult = await reader.ReadAsync<PartnerSubscriptionList>();
                   
                        foreach (var item in partnerSubscriptionListResult)
                        {      
                            var solution = new PartnerSubscriptionList
                            {
                                SolutionCode = item.SolutionCode,
                                SolutionDescription = item.SolutionDescription,
                            };

                            subscriptions.Add(solution);
                        }                    
                        outputDto.partnerSubscriptionLists = subscriptions;
                        return Result.Success(outputDto);
                
                    };
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetAllPartnerSubscriptionQuery] {ex.Message}");
                }
                return Result.Failure<GetAllPartnerSubscriptionOutputDTO>(
                            $"Get partner all subscription failed for {request.PartnerCode}."
                        );
            }
        }
    }
}
