using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using CSharpFunctionalExtensions;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.DomainServices;
using System.Collections.Generic;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using System.Linq;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCBusinessProfile, UACAction.View)]
    [Permission(Permission.KYCManagementBusinessProfile.Action_View_Code,
       new int[] { (int)PortalCode.Connect, (int)PortalCode.Admin, (int)PortalCode.Business })]
    internal class GetBusinessProfileQuery : BaseQuery<Result<BusinessProfileOutputDTO>>
    {
        public string LoginId { get; set; }
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<BusinessProfileOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get Business Profile for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);            
        }

        public class GetBusinessProfileQueryHandler : IRequestHandler<GetBusinessProfileQuery, Result<BusinessProfileOutputDTO>>
        {
            private readonly TrangloUserManager _userManager;
            private readonly BusinessProfileService _businessProfileService;
            private readonly IBusinessProfileRepository _repository;
            private readonly IPartnerRepository _partnerRepository;
            private readonly PartnerService _partnerService;
            private readonly ILogger<GetBusinessProfileQuery> _logger;
            private readonly IConfiguration _config;

            public GetBusinessProfileQueryHandler(
                    TrangloUserManager userManager,
                    BusinessProfileService businessProfileService,
                    IBusinessProfileRepository repository,
                    PartnerService partnerService,
                    IPartnerRepository partnerRepository,
                    ILogger<GetBusinessProfileQuery> logger,
                    IConfiguration config
                )
            {
                _userManager = userManager;
                _businessProfileService = businessProfileService;
                _repository = repository;
                _partnerRepository = partnerRepository;
                _partnerService = partnerService;
                _logger = logger;
                _config = config;
            }

            public async Task<Result<BusinessProfileOutputDTO>> Handle(GetBusinessProfileQuery query, CancellationToken cancellationToken)
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(query.LoginId);
                if (applicationUser is CustomerUser)
                {
                    Result<IReadOnlyList<CustomerUserBusinessProfile>> customerUserBusinessProfiles = await _businessProfileService
                                                                                                              .GetCustomerUserBusinessProfilesAsync(
                                                                                                                  (CustomerUser)applicationUser,
                                                                                                                  query.BusinessProfileCode
                                                                                                              );
                    if (customerUserBusinessProfiles.IsFailure)
                    {
                        _logger.LogError("GetCustomerUserBusinessProfilesAsync", customerUserBusinessProfiles.Error);

                        return Result.Failure<BusinessProfileOutputDTO>(
                                    $"Get Business Profile failed for {query.BusinessProfileCode}. {customerUserBusinessProfiles.Error}"
                                );
                    }


                    IReadOnlyList<CustomerUserBusinessProfile> customerUserBusinessProfileList = customerUserBusinessProfiles.Value;
                    CustomerUserBusinessProfile customerUserBusinessProfile = customerUserBusinessProfileList.TryFirst().Value;
                    query.BusinessProfileCode = customerUserBusinessProfile.BusinessProfileCode;


                }

                var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);
                var partnerProfile = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(query.BusinessProfileCode);
                var partnerSubProfile = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerProfile.Id);
                BusinessProfile businessProfile = businessProfileList.Value;
                            




                if (businessProfileList.IsSuccess)
                {
                    BusinessProfileOutputDTO outputDTO = new BusinessProfileOutputDTO();
                    var _connectionString = _config.GetConnectionString("DefaultConnection");

                    using(var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        var reader = await connection.QuerySingleAsync<BusinessProfileOutputDTO>(
                            "dbo.GetBusinessProfileByBusinessProfileCode",
                            new
                            {
                                BusinessProfileCode = businessProfile.Id
                            },
                             null, null, CommandType.StoredProcedure);

                        outputDTO = reader;

                    }

                    if (outputDTO != null)
                    {
                        outputDTO.BusinessProfileConcurrencyToken = businessProfile.BusinessProfileConcurrencyToken;
                        return Result.Success<BusinessProfileOutputDTO>(outputDTO);
                    }
                }

                return Result.Failure<BusinessProfileOutputDTO>("Failed to retrieve business profile.");

            }
        }
    }    
}
