using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCTemplatesManagement, UACAction.Edit)]
    [Permission(Permission.KYCAdministrationTemplatesManagement.Action_Upload_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCAdministrationTemplatesManagement.Action_View_Code })]
    internal class SaveTemplateCommand : BaseCommand<Result<Guid>>
    {
        public int DocumentCategoryCode { get; set; }
        public long RequestId { get; set; }
        public IFormFile uploadedFile { get; set; }
        public long? QuestionnaireCode { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Uploaded New Template";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }
    internal class SaveTemplateCommandHandler : IRequestHandler<SaveTemplateCommand, Result<Guid>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<SaveTemplateCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        public SaveTemplateCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<SaveTemplateCommandHandler>  logger,
                                                      IBusinessProfileRepository repository,
                                                      StorageManager storageManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
        }
   
        public async Task<Result<Guid>> Handle(SaveTemplateCommand request, CancellationToken cancellationToken)
        {
            var documentCategoryInfo = await _repository.GetCategoryInfo(request.DocumentCategoryCode);
            var documentCategoryId = documentCategoryInfo.Id;
            try
            {
                if (request.AdminSolution == Solution.Connect.Id)
                {
                    if (documentCategoryId == 11)
                    {
                        documentCategoryId = 12;
                    }
                    else if (documentCategoryId == 12)
                    {
                        documentCategoryId = 13;
                    }
                    if (documentCategoryId == 12 || documentCategoryId == 13 || documentCategoryId == 39 || documentCategoryId == 59)

                        if ((documentCategoryId == 13 || documentCategoryId == 39 || documentCategoryId == 59) && request.QuestionnaireCode == null)
                        {
                            return Result.Failure<Guid>($"Questionnaire Code is invalid");
                        }
                }
                else if (request.AdminSolution == Solution.Business.Id)
                {
                    if (documentCategoryId == 11)
                    {
                        documentCategoryId = 58;
                    }
                    else if (documentCategoryId == 12)
                    {
                        documentCategoryId = 59;
                    }
                    if (documentCategoryId == 58  || documentCategoryId == 39 || documentCategoryId == 59)

                        if ((documentCategoryId == 13 || documentCategoryId == 39 || documentCategoryId == 59) && request.QuestionnaireCode == null)
                        {
                            return Result.Failure<Guid>($"Questionnaire Code is invalid");
                        }
                }
                

                using (var ms = new MemoryStream())
                    {
                        var extension = Path.GetExtension(request.uploadedFile.FileName);
                        var allowedExtensions = new[] { ".doc", ".docx", ".pdf", ".zip" };

                        var fileSize = request.uploadedFile.Length;
                        int maxFileSizeMB = 30;
                        var maxFileSize = maxFileSizeMB * 1024 * 1024;

                        if (fileSize > maxFileSize)
                        {
                            return Result.Failure<Guid>($"Document file size exceeds {maxFileSizeMB}mb");
                        }
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Result.Failure<Guid>("Document extension is not allowed");
                        }

                        request.uploadedFile.CopyTo(ms);
                        ms.Position = 0;
                        var doc = await _storageManager.StoreAsync(ms, request.uploadedFile.FileName, request.uploadedFile.ContentType);

                        if (doc != null)
                        
                            {
                                var TemplateInfo = await _repository.GetTemplateInfo(documentCategoryId, request.QuestionnaireCode);
                 
                        if (TemplateInfo == null)
                               
                                    {
                                        DocumentCategoryTemplate documentTemplate = new DocumentCategoryTemplate(documentCategoryInfo, request.QuestionnaireCode)
                                        {
                                            DocumentId = doc.DocumentId,
                                        };

                                        await _repository.AddDocumentTemplateAsync(documentTemplate);
                          
                            return Result.Success<Guid>(doc.DocumentId);
                                    }
                                
                                else
                                {
                                    TemplateInfo.DocumentId = doc.DocumentId;
                            await _repository.UpdateDocumentTemplateAsync(TemplateInfo);
                                    return Result.Success<Guid>(doc.DocumentId);
                                }
                            }   
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SaveTemplateCommand] {ex.Message}");
            }

                return Result.Failure<Guid>("Unable to upload template.");
            }
        }
}
