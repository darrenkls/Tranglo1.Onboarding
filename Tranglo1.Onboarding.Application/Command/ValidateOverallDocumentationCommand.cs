using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.BoardofDirector;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.DTO.LegalEntitiy;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.AuthorisedPerson;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.Shareholder;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    //[Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
    //    new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
    //    new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    public class ValidateOverallDocumentationCommand : BaseCommand<ValidateOverallOutputDTO>
    {
        public int BusinessProfileCode { get; set; }

        public bool FromComment { get; set; }

    }
    public class ValidateOverallDocumentationCommandHandler : IRequestHandler<ValidateOverallDocumentationCommand, ValidateOverallOutputDTO>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IConfiguration _config;

        public ValidateOverallDocumentationCommandHandler(IBusinessProfileRepository businessProfileRepository, IConfiguration configuration)
        {
            _businessProfileRepository = businessProfileRepository;
            _config = configuration;
        }
        public async Task<ValidateOverallOutputDTO> Handle(ValidateOverallDocumentationCommand request, CancellationToken cancellationToken)
        {

            IEnumerable<DocumentOverallOutputDTO> documentations;

            var _connectionString = _config.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var reader = await connection.QueryMultipleAsync(
                       "dbo.GetOverallDocumentation",
                       new
                       {
                           BusinessProfileCode = request.BusinessProfileCode
                       },
                        null, null,
                         CommandType.StoredProcedure);
                documentations = await reader.ReadAsync<DocumentOverallOutputDTO>();
            }


            // Find latest modified for each list and convert to Malaysia time (+8)
            DateTime? documentationDateModified = documentations?
                .Where(x => x.CreatedDate != null)
                .Max(x => x.CreatedDate)?
                .AddHours(8); // or .AddHours(8) if the DateTime.Kind is UTC

            bool isModifiedToday = documentationDateModified?.Date == DateTime.Now.Date;
            string result = isModifiedToday ? "yes" : "no";





            //kycSummaryFeedback
            var isResolved = false;
            if (request.FromComment)
            {
                var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(request.BusinessProfileCode);

                foreach (var i in kycSummaryFeedbackInfo)
                {
                    if (i.IsResolved == false && i.KYCCategory.Id == KYCCategory.Business_Documentation.Id)
                    {
                        isResolved= i.IsResolved = true; //set isResolved to true
                        await _businessProfileRepository.SaveKYCSummaryFeedback(i);
                    }
                }
            }


            // Build output DTO
            var output = new ValidateOverallOutputDTO
            {
                BusinessProfileCode = request.BusinessProfileCode,
                IsResolved = isResolved,
                DocumentationDateModified = documentationDateModified?.ToString("dd MMM yyyy, hh:mm tt '(GMT+8)'"),
                DocumentationLatestModifiedToday = result

            };

            return output;

        }
    }
}
