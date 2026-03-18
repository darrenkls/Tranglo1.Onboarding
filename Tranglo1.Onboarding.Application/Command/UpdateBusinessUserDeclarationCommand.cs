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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Declaration;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class UpdateBusinessUserDeclarationCommand : BaseCommand<Result<BusinessUserDeclarationOutputDTO>>
    {
        public BusinessUserDeclarationInputDTO InputDTO;
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? BusinessUserDeclarationConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<BusinessUserDeclarationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Updating Business User Declaration for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        internal class UpdateBusinessUserDeclarationCommandHandler : IRequestHandler<UpdateBusinessUserDeclarationCommand, Result<BusinessUserDeclarationOutputDTO>>
        {
            private readonly IBusinessProfileRepository _repository;
            private readonly ILogger<UpdateBusinessUserDeclarationCommandHandler> _logger;
            private readonly TrangloUserManager _userManager;
            private readonly IPartnerRepository _partnerRepository;
            private readonly BusinessProfileService _businessProfileService;
            private readonly PartnerService _partnerService;

            public UpdateBusinessUserDeclarationCommandHandler(
                IBusinessProfileRepository repository,
                ILogger<UpdateBusinessUserDeclarationCommandHandler> logger,
                TrangloUserManager userManager,
                IPartnerRepository partnerRepository,
                BusinessProfileService businessProfileService,
                PartnerService partnerService)
            {
                _repository = repository;
                _logger = logger;
                _userManager = userManager;
                _partnerRepository = partnerRepository;
                _businessProfileService = businessProfileService;
                _partnerService = partnerService;
            }

            public async Task<Result<BusinessUserDeclarationOutputDTO>> Handle(UpdateBusinessUserDeclarationCommand request, CancellationToken cancellationToken)
            {
                var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
                var businessUserDeclaration = await _repository.GetBusinessUserDeclarationByBusinessProfileCodeAsync(businessProfile.Id);

                Result<BusinessUserDeclarationOutputDTO> result = null;
                // Handle concurrency
                if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
                {
                    var concurrencyCheck = ConcurrencyCheck(request.BusinessUserDeclarationConcurrencyToken, businessUserDeclaration);
                    if (concurrencyCheck.IsFailure)
                    {
                        return Result.Failure<BusinessUserDeclarationOutputDTO>(concurrencyCheck.Error);
                    }
                }


                if (request.AdminSolution != null || request.CustomerSolution != null)
                {
                    if (ClaimCode.Connect == request.CustomerSolution)
                    {
                        return Result.Failure<BusinessUserDeclarationOutputDTO>(
                            $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                        );
                    }
                    else if (ClaimCode.Business == request.CustomerSolution)
                    {
                        result = await UpdateBusinessUserDeclaration(request, businessProfile, businessUserDeclaration); // Call UpdateCustomerVerification with null customerVerification

                        if (result.IsFailure)
                        {
                            return Result.Failure<BusinessUserDeclarationOutputDTO>(
                                $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                            );
                        }
                    }
                    else if (Solution.Connect.Id == request.AdminSolution)
                    {
                        return Result.Failure<BusinessUserDeclarationOutputDTO>(
                            $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                        );
                    }
                    else if (Solution.Business.Id == request.AdminSolution)
                    {
                        result = await UpdateBusinessUserDeclaration(request, businessProfile, businessUserDeclaration); // Call UpdateCustomerVerification with null customerVerification

                        if (result.IsFailure)
                        {
                            return Result.Failure<BusinessUserDeclarationOutputDTO>(
                                $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                            );
                        }
                    }
                    else
                    {
                        return Result.Failure<BusinessUserDeclarationOutputDTO>(
                            $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                        );
                    }

                    return result;
                }
                else
                {

                    return Result.Failure<BusinessUserDeclarationOutputDTO>("Invalid request");
                }
            }

            private async Task<Result<BusinessUserDeclarationOutputDTO>> UpdateBusinessUserDeclaration(UpdateBusinessUserDeclarationCommand request, BusinessProfile businessProfile, BusinessUserDeclaration businessUserDeclaration)
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

                if (businessUserDeclaration == null)
                {
                    return Result.Failure<BusinessUserDeclarationOutputDTO>("Business user declaration does not exist.");
                }

                // Generate a new concurrency token for first-time creation or when COInfo is null
                Guid newConcurrencyToken = businessUserDeclaration?.BusinessUserDeclarationConcurrencyToken ?? Guid.NewGuid();

                    businessUserDeclaration.IsNotRemittancePartner = request.InputDTO.IsNotRemittancePartner ?? businessUserDeclaration.IsNotRemittancePartner;
                    businessUserDeclaration.IsAuthorized = request.InputDTO.IsAuthorized ?? businessUserDeclaration.IsAuthorized;
                    businessUserDeclaration.IsInformationTrue = request.InputDTO.IsInformationTrue ?? businessUserDeclaration.IsInformationTrue;
                    businessUserDeclaration.IsAgreedTermsOfService = request.InputDTO.IsAgreedTermsOfService ?? businessUserDeclaration.IsAgreedTermsOfService;
                    businessUserDeclaration.IsDeclareTransactionTax = request.InputDTO.IsDeclareTransactionTax ?? businessUserDeclaration.IsDeclareTransactionTax;
                    businessUserDeclaration.IsAllApplicationAccurate = request.InputDTO.IsAllApplicationAccurate ?? businessUserDeclaration.IsAllApplicationAccurate;
                    businessUserDeclaration.SigneeName = request.InputDTO.SigneeName ?? businessUserDeclaration.SigneeName;
                    businessUserDeclaration.Designation = request.InputDTO.Designation ?? businessUserDeclaration.Designation;
                    businessUserDeclaration.BusinessUserDeclarationConcurrencyToken = newConcurrencyToken;
                

                await _repository.UpdateBusinessUserDeclarationAsync(businessUserDeclaration);
                
                BusinessUserDeclarationOutputDTO outputDTO = new BusinessUserDeclarationOutputDTO
                {
                    BusinessProfileCode = businessProfile.Id,
                    BusinessUserDeclarationCode = businessUserDeclaration.Id,
                    IsNotRemittancePartner = businessUserDeclaration.IsNotRemittancePartner,
                    IsAuthorized = businessUserDeclaration.IsAuthorized,
                    IsInformationTrue = businessUserDeclaration.IsInformationTrue,
                    IsAgreedTermsOfService = businessUserDeclaration.IsAgreedTermsOfService,
                    IsDeclareTransactionTax = businessUserDeclaration.IsDeclareTransactionTax,
                    IsAllApplicationAccurate = businessUserDeclaration.IsAllApplicationAccurate,
                    SigneeName = businessUserDeclaration.SigneeName,
                    Designation = businessUserDeclaration.Designation,
                    BusinessUserDeclarationConcurrencyToken = newConcurrencyToken
                };

                return Result.Success<BusinessUserDeclarationOutputDTO>(outputDTO);


            }

            private Result ConcurrencyCheck(Guid? concurrencyToken, BusinessUserDeclaration businessUserDeclaration)
            {
                try
                {
                    if ((concurrencyToken.HasValue && businessUserDeclaration?.BusinessUserDeclarationConcurrencyToken != concurrencyToken) ||
                        concurrencyToken is null && businessUserDeclaration?.BusinessUserDeclarationConcurrencyToken != null)
                    {
                        // Return a 409 Conflict status code when there's a concurrency issue

                        return Result.Failure<BusinessUserDeclarationOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                    }
                    businessUserDeclaration.BusinessUserDeclarationConcurrencyToken = Guid.NewGuid();
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while processing the request.");

                    // Return a 409 Conflict status code
                    return Result.Failure<BusinessUserDeclarationOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }
            }

        }
    }
}
