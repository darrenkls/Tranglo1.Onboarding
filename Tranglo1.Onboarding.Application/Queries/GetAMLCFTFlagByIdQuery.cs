using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementAMLCFT.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect , (int)PortalCode.Business },
        new string[] { })]
    class GetAMLCFTFlagByIdQuery : BaseCommand<AMLCFTFlagOutputDTO>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(AMLCFTFlagOutputDTO result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get AMLCFT Flag for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get AMLCFT Flag for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetAMLCFTFlagByIdQueryHandler : IRequestHandler<GetAMLCFTFlagByIdQuery, AMLCFTFlagOutputDTO>
        {
            private readonly IConfiguration _config;
            private readonly BusinessProfileService _businessProfileService;
            private readonly TrangloUserManager _userManager;
            public GetAMLCFTFlagByIdQueryHandler(IConfiguration config, BusinessProfileService businessProfileService, TrangloUserManager userManager)
            {
                _config = config;
                _businessProfileService = businessProfileService;
                _userManager = userManager;
            }

            async Task<AMLCFTFlagOutputDTO> IRequestHandler<GetAMLCFTFlagByIdQuery, AMLCFTFlagOutputDTO>.Handle(GetAMLCFTFlagByIdQuery request, CancellationToken cancellationToken)
            {
                //LoginId
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

                var adminSolution = request.AdminSolution is null ? long.MinValue : request.AdminSolution.Value;
                var solution = Enumeration.FindById<Solution>(adminSolution);

                if( applicationUser is CustomerUser )
                {
                    //ClaimCode.Business
                    solution = request.CustomerSolution == ClaimCode.Business ? Solution.Business : Solution.Connect;
                }

                //1. Get from AMLCFT Documentation based on the business profile
                bool isExistUploadedAMLDocumentaton = await _businessProfileService.CheckHasUploadedAMLDocumentation(request.BusinessProfileCode, solution);

                //2. Get from AML CFT Questionnaire basd on the business profile
                bool isExistAnsweredAMLQuestionnaire = await _businessProfileService.CheckHasAnsweredAMLQuestionnaire(request.BusinessProfileCode);

                AMLCFTFlagOutputDTO aMLCFTFlagOutputDTO = new AMLCFTFlagOutputDTO()
                {
                    HasAnsweredAMLQuestionnaire = isExistAnsweredAMLQuestionnaire,
                    HasUploadedAMLDocumentation = isExistUploadedAMLDocumentaton
                };

                return aMLCFTFlagOutputDTO;
            }
        }

    }
}
