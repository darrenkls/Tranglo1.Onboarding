using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.ParentHoldingCompany;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SaveParentHoldingCompanyCommand : BaseCommand<Result<IEnumerable<ParentHoldingCompanyOutputDTO>>>
    {
        public IEnumerable<ParentHoldingCompanyInputDTO> ParentHoldingCompany;
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? ParentHoldingsConcurrencyToken { get; set; }


        public override Task<string> GetAuditLogAsync(Result<IEnumerable<ParentHoldingCompanyOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Parent Holding Companies for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveParentHoldingCompanyCommandHandler : IRequestHandler<SaveParentHoldingCompanyCommand, Result<IEnumerable<ParentHoldingCompanyOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveParentHoldingCompanyCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IScreeningRepository _screeningRepository;

        public SaveParentHoldingCompanyCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SaveParentHoldingCompanyCommandHandler> logger,
                IMapper mapper,
                PartnerService partnerService,
                IBusinessProfileRepository businessProfileRepository,
                IScreeningRepository screeningRepository
            )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _screeningRepository = screeningRepository;
        }

        public async Task<Result<IEnumerable<ParentHoldingCompanyOutputDTO>>> Handle(SaveParentHoldingCompanyCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<ParentHoldingCompanyOutputDTO>>("Invalid Business Profile");
            }

            var businessProfile = businessProfileList.Value;
            Result<IEnumerable<ParentHoldingCompanyOutputDTO>> result = new Result<IEnumerable<ParentHoldingCompanyOutputDTO>>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

            // Handle concurrency
            if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
            {
                var concurrencyCheck = ConcurrencyCheck(request.ParentHoldingsConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ParentHoldingCompanyOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }

            if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete ||
                kycBusinessReviewResult != null))
            {
                //update
                result = await UpdateParentHolding(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ParentHoldingCompanyOutputDTO>>(
                                        $"Customer user is unable to update for {request.BusinessProfileCode}."
                                        );
                }
            }

            else if (applicationUser is TrangloStaff &&
                ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) || 
                businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                || kycBusinessReviewResult != null)))
            {
                //update
                result = await UpdateParentHolding(request, businessProfile, cancellationToken);
 
                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ParentHoldingCompanyOutputDTO>>(
                                        $"Admin user is unable to update for {request.BusinessProfileCode}."
                                        );
                }

                //check mandatory fields
                //temporary commenting due to design issue on API causing deadlock
                //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
            }

            else
            {
                return Result.Failure<IEnumerable<ParentHoldingCompanyOutputDTO>>(
                                         $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                         );
            }

            return result;
        }

        private async Task<Result<IEnumerable<ParentHoldingCompanyOutputDTO>>> UpdateParentHolding(SaveParentHoldingCompanyCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            var parentHolding = await _businessProfileService.GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile);

            IReadOnlyList<ParentHoldingCompany> parentHoldingCompanies =
                parentHolding.IsSuccess ? parentHolding.Value : Enumerable.Empty<ParentHoldingCompany>().ToList().AsReadOnly();

            
            //To Add Values

            foreach (var inputParentHolding in request.ParentHoldingCompany)
            {
                var country =await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputParentHolding.CountryISO2);




                if (inputParentHolding.ParentHoldingCompanyCode == 0 || inputParentHolding.ParentHoldingCompanyCode == null)
                {
                    ParentHoldingCompany parentHoldingCompany =
                         new ParentHoldingCompany(businessProfile, inputParentHolding.NameOfListedParentHoldingCompany,
                                         country, inputParentHolding.NameOfStockExchange, inputParentHolding.StockCode, inputParentHolding.DateOfIncorporation);

                    await _businessProfileService.AddParentHoldingCompanyAsync(businessProfile, parentHoldingCompany);

                }

                //To Update Values
                else if(!inputParentHolding.IsDeleted)
                {
                    var _ExistingParentHolding = parentHoldingCompanies
                        .Where(x => x.Id == inputParentHolding.ParentHoldingCompanyCode)
                        .FirstOrDefault();

                    if (_ExistingParentHolding == null)
                    {
                        return Result.Failure<IEnumerable<ParentHoldingCompanyOutputDTO>>(
                            $"Invalid Parent Holding Company Code [{inputParentHolding.ParentHoldingCompanyCode}].");
                    }
 
                    _ExistingParentHolding.Country = country;
                    _ExistingParentHolding.NameOfListedParentHoldingCompany = inputParentHolding.NameOfListedParentHoldingCompany;
                    _ExistingParentHolding.NameOfStockExchange = inputParentHolding.NameOfStockExchange;
                    _ExistingParentHolding.StockCode = inputParentHolding.StockCode;
                    _ExistingParentHolding.DateOfIncorporation = inputParentHolding.DateOfIncorporation;

                    await _businessProfileService.UpdateParentHoldingCompanyAsync(businessProfile, _ExistingParentHolding, cancellationToken);
                }
            }

            //TO Delete Values
            var deletedParentHoldings = from existing in parentHoldingCompanies
                                        let fromInput = request.ParentHoldingCompany
                                        .FirstOrDefault(input =>
                                        input.IsDeleted &&
                                        input.ParentHoldingCompanyCode.HasValue &&
                                        input.ParentHoldingCompanyCode.Value == existing.Id
                                        )
                                        where fromInput?.ParentHoldingCompanyCode == existing?.Id
                                        select existing;

            if (deletedParentHoldings.Any())
            {
                List<long> deletedParentHoldingsIds = deletedParentHoldings.Select(existing => existing.Id).ToList();

                await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedParentHoldingsIds,businessProfile.Id);
                await _businessProfileService.DeleteParentHoldingCompanyAsync(businessProfile, deletedParentHoldings, cancellationToken);
            }

            parentHolding = await _businessProfileService.GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile);
            var _isParentHoldingCompleted = await _businessProfileService.IsOwnershipParentHoldingCompleted(businessProfile.Id);

            // Check if any Parent Holdings remain after updates or deletions
            var remainingParentHoldings = await _businessProfileService.GetParentHoldingCompanyByBusinessProfileCodeAsync(businessProfile);

            // After updates or deletions
            if (remainingParentHoldings.IsSuccess && !remainingParentHoldings.Value.Any())
            {
                // All Parent Holdings are deleted, set the concurrency token to null
                businessProfile.ParentHoldingsConcurrencyToken = null;
            }
            else
            {
                // Update the concurrency token since some  Parent Holdings are still present
                businessProfile.ParentHoldingsConcurrencyToken = Guid.NewGuid();
            }

            // Update the business profile
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            return Result.Success<IEnumerable<ParentHoldingCompanyOutputDTO>>(parentHolding.Value.Select(s => new ParentHoldingCompanyOutputDTO()
            {
                NameOfListedParentHoldingCompany = s.NameOfListedParentHoldingCompany,
                CountryISO2 = s.Country?.CountryISO2,
                NameOfStockExchange = s.NameOfStockExchange,
                StockCode = s.StockCode,
                DateOfIncorporation = s.DateOfIncorporation,
                ParentHoldingCompanyCode = s.Id,
                isCompleted = _isParentHoldingCompleted,
                ParentHoldingsConcurrencyToken = businessProfile.ParentHoldingsConcurrencyToken
                
            }));
        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.ParentHoldingsConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.ParentHoldingsConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<ParentHoldingCompanyOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.ParentHoldingsConcurrencyToken = Guid.NewGuid();
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<ParentHoldingCompanyOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }
        }
    }    
}
