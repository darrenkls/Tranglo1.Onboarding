using AutoMapper;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerSubscriptionListQuery : BaseQuery<Result<GetSalesPartnerSubscriptionOutputDTO>>
    {
        public long PartnerCode { get; set; }
    }

    internal class GetPartnerSubscriptionCodeListQueryHandler : IRequestHandler<GetPartnerSubscriptionListQuery, Result<GetSalesPartnerSubscriptionOutputDTO>>
    {
        private readonly IPartnerRepository _partnerRepository;
     
        public GetPartnerSubscriptionCodeListQueryHandler(IMapper mapper, IConfiguration config, IHttpClientFactory httpClientFactory, IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<GetSalesPartnerSubscriptionOutputDTO>> Handle(GetPartnerSubscriptionListQuery request, CancellationToken cancellationToken)
        {
            var partnerSubscriptionListoutputDTO = new GetSalesPartnerSubscriptionOutputDTO();
            var subscriptionList = new List<SubscriptionDetail>();

            var querySalesSubscriptionList = await _partnerRepository.GetPartnerSubscriptionListAsync(request.PartnerCode);

            if (querySalesSubscriptionList.Count == 0)
            {
                return Result.Failure<GetSalesPartnerSubscriptionOutputDTO>($"No subscription exist for partner {request.PartnerCode}");
            }

            querySalesSubscriptionList.ForEach(details => subscriptionList.Add(new SubscriptionDetail(details.Id, details.TrangloEntity, details.PartnerType?.Name, details.Solution?.Name, details.SettlementCurrencyCode)));

            partnerSubscriptionListoutputDTO.PartnerCode = request.PartnerCode;
            partnerSubscriptionListoutputDTO.PartnerSubscriptionDetails = subscriptionList;

            return partnerSubscriptionListoutputDTO;
        }
    }
}
