using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Tranglo1.Onboarding.Application.DTO.CommentAndReviewRemarks;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;
using Tranglo1.DocumentStorage;
using System.IO;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    [Permission(Permission.KYCManagementDocumentation.Action_ReviewRemark_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class GetReviewRemarksCommentsByIdQuery : BaseQuery<IEnumerable<CommentAndReviewRemarksOutputDTO>> 
   {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public int IsExternal { get; set; }
        public Guid DocumentId { get; set; }
        public string FileName { get; set; }


        public override Task<string> GetAuditLogAsync(IEnumerable<CommentAndReviewRemarksOutputDTO> result)
        {
            /*
            if (result.IsSuccess)
            {
                string _description = $"Get Review Remarks for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
            */

            string _description = $"Get Review Remarks for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetReviewRemarksByIdQueryHandler : IRequestHandler<GetReviewRemarksCommentsByIdQuery, IEnumerable<CommentAndReviewRemarksOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetReviewRemarksByIdQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<IEnumerable<CommentAndReviewRemarksOutputDTO>> Handle(GetReviewRemarksCommentsByIdQuery request, CancellationToken cancellationToken)
            {
          
                var _connectionString = _config.GetConnectionString("DefaultConnection");

                IEnumerable<CommentAndReviewRemarksOutputDTO> reviewRemarksCommentDTOs;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetDocumentCommentRemarkPerCategory",
                        new {
                            BusinessProfileCode = request.BusinessProfileCode,
                            DocumentCategoryCode = request.DocumentCategoryCode,
                            IsExternal = request.IsExternal,
                        },
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    reviewRemarksCommentDTOs = await reader.ReadAsync<CommentAndReviewRemarksOutputDTO>();
                }

                return reviewRemarksCommentDTOs;
            }
        }        
    }
}
