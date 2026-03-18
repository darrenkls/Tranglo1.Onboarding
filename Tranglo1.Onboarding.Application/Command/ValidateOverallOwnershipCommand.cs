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
    public class ValidateOverallOwnershipCommand : BaseCommand<ValidateOverallOutputDTO>
    {
        public int BusinessProfileCode { get; set; }


        
    }
    public class ValidateOverallOwnershipCommandHandler : IRequestHandler<ValidateOverallOwnershipCommand, ValidateOverallOutputDTO>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IConfiguration _config;

        public ValidateOverallOwnershipCommandHandler(IBusinessProfileRepository businessProfileRepository, IConfiguration configuration)
        {
            _businessProfileRepository = businessProfileRepository;
            _config = configuration;
        }
        public async  Task<ValidateOverallOutputDTO> Handle(ValidateOverallOwnershipCommand request, CancellationToken cancellationToken)
        {
            
            IEnumerable<ShareholderOutputDTO> shareholders;
            IEnumerable<BoardofDirectorOutputDTO> boardofDirectors;
            IEnumerable<AuthorisedPersonOutputDTO> authoriseds;
            IEnumerable<LegalEntitiyOutputDTO> ubo;

            var _connectionString = _config.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var reader = await connection.QueryMultipleAsync(
                       "dbo.GetOverallOwnership",
                       new
                       {
                           BusinessProfileCode = request.BusinessProfileCode
                       },
                        null, null,
                         CommandType.StoredProcedure);
                shareholders = await reader.ReadAsync<ShareholderOutputDTO>();
                boardofDirectors = await reader.ReadAsync<BoardofDirectorOutputDTO>();
                authoriseds = await reader.ReadAsync<AuthorisedPersonOutputDTO>();
                ubo = await reader.ReadAsync<LegalEntitiyOutputDTO>();
            }


            // Find latest modified for each list and convert to Malaysia time (+8)
            DateTime? shareholderDateModified = shareholders?
                .Where(x => x.LastModifiedDate != null)
                .Max(x => x.LastModifiedDate)?
                .AddHours(8); // or .AddHours(8) if the DateTime.Kind is UTC

            DateTime? boardOfDirectorDateModified = boardofDirectors?
                .Where(x => x.LastModifiedDate != null)
                .Max(x => x.LastModifiedDate)?
                .AddHours(8);

            DateTime? authorisedPersonDateModified = authoriseds?
                .Where(x => x.LastModifiedDate != null)
                .Max(x => x.LastModifiedDate)?
                .AddHours(8);

            DateTime? uboDateModified = ubo?
                .Where(x => x.LastModifiedDate != null)
                .Max(x => x.LastModifiedDate)?
                .AddHours(8);

            //Update KYCSummaryFeedback
            var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var isResolved = false;
            foreach (var i in kycSummaryFeedbackInfo)
            {
                if (i.IsResolved == false && i.KYCCategory.Id == KYCCategory.Business_Ownership.Id)
                {
                    isResolved = i.IsResolved = true; //set isResolved to true
                    await _businessProfileRepository.SaveKYCSummaryFeedback(i);
                }
            }


            // Build output DTO
            var output = new ValidateOverallOutputDTO
            {
                BusinessProfileCode = request.BusinessProfileCode,
                IsResolved = isResolved,
                ShareholderDateModified = shareholderDateModified?.ToString("dd MMM yyyy, hh:mm tt '(GMT+8)'"),
                BoardOfDirectorDateModified = boardOfDirectorDateModified?.ToString("dd MMM yyyy, hh:mm tt '(GMT+8)'"),
                AuthorisedPersonDateModified = authorisedPersonDateModified?.ToString("dd MMM yyyy, hh:mm tt '(GMT+8)'"),
                UBODateModified = uboDateModified?.ToString("dd MMM yyyy, hh:mm tt '(GMT+8)'"),

            };

            return output;

        }
    }
}
