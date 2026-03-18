using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class GetTermsAndConditionsAcceptanceStatusQuery : IRequest<Result<bool>>
	{
		public string RoleCode { get; set; }
		public DateTime TermsAcceptanceDate { get; set; }
		public long PartnerCode { get; set; }
	

		public class GetTermsAndConditionsAcceptanceStatusQueryHandler : IRequestHandler<GetTermsAndConditionsAcceptanceStatusQuery, Result<bool>>
		{
			private readonly IPartnerRepository _partnerRepository;
			private readonly IExternalUserRoleRepository _externalUserRoleRepository;

			public GetTermsAndConditionsAcceptanceStatusQueryHandler(IPartnerRepository partnerRepository,IExternalUserRoleRepository externalUserRoleRepository)
			{
				this._partnerRepository = partnerRepository;
				this._externalUserRoleRepository = externalUserRoleRepository;
			}

			public async Task<Result<bool>> Handle(GetTermsAndConditionsAcceptanceStatusQuery request, CancellationToken cancellationToken)
			{
				var partnerAdminRole = await _externalUserRoleRepository.GetExternalRoleByRoleCodeAsync(request.RoleCode);

				var partnerInfo = await _partnerRepository.GetPartnerRegistrationByCodeAsync(request.PartnerCode);

				bool acceptanceDateUpdateRequired = IsAcceptanceDateUpdateRequired(partnerInfo,request.TermsAcceptanceDate);

				bool hasValidRole = partnerAdminRole?.ExternalUserRoleName == "System Admin";

				bool isAccepted = hasValidRole && acceptanceDateUpdateRequired;

				return Result.Success(isAccepted);
			}

			private bool IsAcceptanceDateUpdateRequired(PartnerRegistration partnerInfo, DateTime termsAcceptanceDate)
			{
				return partnerInfo.TermsAcceptanceDate is null || partnerInfo.TermsAcceptanceDate < termsAcceptanceDate;
			}
		}
        }
	}

