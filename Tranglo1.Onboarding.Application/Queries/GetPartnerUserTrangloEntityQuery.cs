using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetPartnerUserTrangloEntityQuery : BaseQuery<Result<GetPartnerUserTrangloEntityOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<GetPartnerUserTrangloEntityOutputDTO> result)
        {
            string _description = $"Get Partner Tranglo Entity  for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

    }

    internal class GetPartnerUserTrangloEntityQueryHandler : IRequestHandler<GetPartnerUserTrangloEntityQuery, Result<GetPartnerUserTrangloEntityOutputDTO>>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<GetPartnerUserTrangloEntityQueryHandler> _logger;

        public GetPartnerUserTrangloEntityQueryHandler(IBusinessProfileRepository businessProfileRepository, IPartnerRepository partnerRepository,
            ILogger<GetPartnerUserTrangloEntityQueryHandler> logger)
        {
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
            _logger = logger;
        }

        public async Task<Result<GetPartnerUserTrangloEntityOutputDTO>> Handle(GetPartnerUserTrangloEntityQuery request, CancellationToken cancellationToken)
        {
            var partner = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            if (partner == null)
            {

                return Result.Failure<GetPartnerUserTrangloEntityOutputDTO>($"Partner code not found for Business Profile Code: {request.BusinessProfileCode}");
            }
            var partnerCode = await _partnerRepository.GetPartnerRegistrationCodeByCodeAsync(partner.Id);


            var solutionMapping = new Dictionary<string, string>
                {
                    { ClaimCode.Connect, Solution.Connect.Name },
                    { ClaimCode.Business, Solution.Business.Name }
                };

            var customerSolution = request.CustomerSolution?.ToLower();



            if (!string.IsNullOrEmpty(customerSolution) && solutionMapping.TryGetValue(customerSolution, out var mappedSolution))
            {
                // The customerSolution is valid and mapped to the corresponding solution name
                var trangloEntity = await _partnerRepository.GetPartnerUserTrangloEntityByCodeAsync(partnerCode.Id, mappedSolution);

                if (trangloEntity == null)
                {
                    return Result.Failure<GetPartnerUserTrangloEntityOutputDTO>($"Customer TrangloEntity not found for Partner Code: {partnerCode} and Customer Solution: {request.CustomerSolution}");
                }


                if (trangloEntity != null)
                {
                    var outputDTO = new GetPartnerUserTrangloEntityOutputDTO
                    {
                       PartnerCode = trangloEntity.PartnerCode,
                       SolutionCode = trangloEntity.Solution?.Id,
                       SolutionDescription = trangloEntity.Solution?.Name,
                        TrangloEntity = trangloEntity.TrangloEntity
                    };

                    return outputDTO;
                }
            }
            else
            {
                // Handle unknown customer solution code
                throw new ArgumentException("Unknown customer solution code");
            }

           


            return Result.Failure<GetPartnerUserTrangloEntityOutputDTO>($"Error Retrieving Tranglo Entity");


        }
    }
}
