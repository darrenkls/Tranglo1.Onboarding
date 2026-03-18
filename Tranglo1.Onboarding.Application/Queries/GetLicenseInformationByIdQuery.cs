using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Application.DTO.LicenseInformation;
using Tranglo1.Onboarding.Application.MediatR;
using CSharpFunctionalExtensions;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.DocumentStorage;
using System.IO;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCLicenseInformation, UACAction.View)]
    [Permission(Permission.KYCManagementLicenseInformation.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] {})]
    internal class GetLicenseInformationByIdQuery : BaseQuery<Result<LicenseInformationOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<LicenseInformationOutputDTO> result)
        {
            
            if (result.IsSuccess)
            {
                string _description = $"Get License Information for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            
            /*
            string _description = $"Get License Information for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
            */
        }

        public class GetLicenseInformationByIdQueryHandler : IRequestHandler<GetLicenseInformationByIdQuery, Result<LicenseInformationOutputDTO>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly IMapper _mapper;
            private readonly StorageManager _storageManager;

            public GetLicenseInformationByIdQueryHandler( IMapper mapper,BusinessProfileService businessProfileService, StorageManager storageManager)
            {
                _businessProfileService = businessProfileService;
                _mapper = mapper;
                _storageManager = storageManager;
            }
                
            public async Task<Result<LicenseInformationOutputDTO>> Handle(GetLicenseInformationByIdQuery request, CancellationToken cancellationToken)
            {
                var licenseInformation = await _businessProfileService.GetLicenseInfoByBusinessCode(request.BusinessProfileCode);
                var result = _mapper.Map<LicenseInformationOutputDTO>(licenseInformation);

                if (licenseInformation != null && licenseInformation.RegulatorDocumentId != null)
                {
                    var document = await _storageManager.GetDocumentMetadataAsync(result.RegulatorDocumentId.Value);
                    if (document != null)
                    {
                        var ms = new MemoryStream();
                        await _storageManager.CopyToAsync(document.DocumentId, ms);
                        ms.Position = 0;
                        result.FileSizeBytes = document.Length;
                    }
                }
                else if (licenseInformation == null)
                {
                    return null;
                }

                return Result.Success(result);
            }
        }
    }
}