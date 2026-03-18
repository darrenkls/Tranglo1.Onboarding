using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    ////[Permission("GetKYCSubmissionStatusByIdQuery", "Get KYC Submission Status By Query", "GetKYCSubmissionStatusByIdQuery")]

    internal class GetKYCSubmissionStatusByIdQuery : BaseQuery<Result<GetKYCSubmissionStatusOutputDto>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<GetKYCSubmissionStatusOutputDto> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Get KYC Submission Status for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class GetKYCSubmissionStatusByIdQueryHandler : IRequestHandler<GetKYCSubmissionStatusByIdQuery, Result<GetKYCSubmissionStatusOutputDto>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<GetKYCSubmissionStatusByIdQuery> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;



        public GetKYCSubmissionStatusByIdQueryHandler(BusinessProfileService businessProfileService,
                                             ILogger<GetKYCSubmissionStatusByIdQuery> logger,
                                              IMapper mapper, TrangloUserManager userManager
)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;

        }

        public async Task<Result<GetKYCSubmissionStatusOutputDto>> Handle(GetKYCSubmissionStatusByIdQuery query, CancellationToken cancellationToken)
        {
            var businessProfiles = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(query.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(query.LoginId);

            if (businessProfiles.IsSuccess && businessProfiles.Value != null)
            {
                KYCSubmissionStatus submissionStatus = null;

                // Check if the user is a TrangloStaff and AdminSolution is specified
                if (applicationUser is TrangloStaff)
                {
                    // Use AdminSolution logic here
                    // Temporary cater for null values until FE has refactored all areas calling this API
                    if (query.AdminSolution == Solution.Connect.Id || (query.AdminSolution == null && string.IsNullOrEmpty(query.CustomerSolution)))
                    {
                        submissionStatus = KYCSubmissionStatus.FindById<KYCSubmissionStatus>(businessProfiles.Value.KYCSubmissionStatusCode.GetValueOrDefault());
                    }
                    else if (query.AdminSolution == Solution.Business.Id)
                    {
                        submissionStatus = KYCSubmissionStatus.FindById<KYCSubmissionStatus>(businessProfiles.Value.BusinessKYCSubmissionStatus?.Id ?? 0);
                    }
                }
                else // If the user is not a TrangloStaff or AdminSolution is not specified, use the user's solution
                {
                    // Use User's Solution logic here
                    if (query.CustomerSolution == ClaimCode.Connect)
                    {
                        submissionStatus = KYCSubmissionStatus.FindById<KYCSubmissionStatus>(businessProfiles.Value.KYCSubmissionStatusCode.GetValueOrDefault());
                    }
                    else if (query.CustomerSolution == ClaimCode.Business)
                    {
                        submissionStatus = KYCSubmissionStatus.FindById<KYCSubmissionStatus>(businessProfiles.Value.BusinessKYCSubmissionStatus?.Id ?? 0);
                    }
                }

                var submission = new GetKYCSubmissionStatusOutputDto();

                if (submissionStatus != null)
                {
                    submission.SubmissionStatusCode = submissionStatus.Id;
                    submission.SubmissionStatusDescription = submissionStatus.Name;                    

                    return Result.Success(submission);
                }
                else
                {
                    // Return draft to cater for null submission status
                    submission.SubmissionStatusCode = KYCSubmissionStatus.Draft.Id;
                    submission.SubmissionStatusDescription = KYCSubmissionStatus.Draft.Name;

                    return Result.Success(submission);
                }
            }

            return Result.Failure<GetKYCSubmissionStatusOutputDto>($"Invalid SubmissionStatusCode for Business Profile Code: {query.BusinessProfileCode}.");
        }
        

        /*   if (businessProfiles.IsSuccess && businessProfiles.Value != null)
           {
               // temporary cater for null values until FE has refactored all areas calling this API
               if ((query.AdminSolution == Solution.Connect.Id || query.AdminSolution is null) && businessProfiles.Value.KYCSubmissionStatusCode != null)
               {
                   var submissionStatus = KYCSubmissionStatus.FindById<KYCSubmissionStatus>(businessProfiles.Value.KYCSubmissionStatusCode.Value);

                   var submission = new GetKYCSubmissionStatusOutputDto()
                   {
                       SubmissionStatusCode = submissionStatus.Id,
                       SubmissionStatusDescription = submissionStatus.Name

                   };
                   return Result.Success(submission);
               }
               else if (query.AdminSolution == Solution.Business.Id && businessProfiles.Value.BusinessKYCSubmissionStatus != null)
               {
                   var submissionStatus = KYCSubmissionStatus.FindById<KYCSubmissionStatus>(businessProfiles.Value.BusinessKYCSubmissionStatus.Id);

                   var submission = new GetKYCSubmissionStatusOutputDto()
                   {
                       SubmissionStatusCode = submissionStatus.Id,
                       SubmissionStatusDescription = submissionStatus.Name

                   };
                   return Result.Success(submission);
               }                    
           }*//*

           {
               return Result.Failure<GetKYCSubmissionStatusOutputDto>(
                           $"Invalid SubmissionStatusCode {query.BusinessProfileCode}." 
                       );
           }*/



    }
}

