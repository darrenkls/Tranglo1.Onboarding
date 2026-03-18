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
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class SaveDefaultTemplateCommand : BaseCommand<Result<Guid>>
    {
        public int DefaultTemplateCode { get; set; }
        public IFormFile uploadedFile { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Save Default Template for DefaultTemplateCode: [{this.DefaultTemplateCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }
    internal class SaveDefaultTemplateCommandHandler : IRequestHandler<SaveDefaultTemplateCommand, Result<Guid>>
    {
        private readonly IBusinessProfileRepository _repository;
        private readonly ILogger<SaveDefaultTemplateCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        public SaveDefaultTemplateCommandHandler(ILogger<SaveDefaultTemplateCommandHandler> logger, IBusinessProfileRepository repository, StorageManager storageManager)
        {
            _repository = repository;
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<Result<Guid>> Handle(SaveDefaultTemplateCommand request, CancellationToken cancellationToken)
        {
            var defaultTemplate = Enumeration.FindById<DefaultTemplate>(request.DefaultTemplateCode);
            if (defaultTemplate is null)
            {
                return Result.Failure<Guid>($"Default template for DefaultTemplateCode: {request.DefaultTemplateCode} does not exist");
            }

            var output = Guid.Empty;

            try
            {                
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

                    // Check if DefaultTemplateDocument exists
                    var defaultTemplateDocument = await _repository.GetDefaultTemplateDocumentAsync(defaultTemplate.Id);
                    if (defaultTemplateDocument != null)
                    {
                        var document = await _storageManager.GetDocumentMetadataAsync(defaultTemplateDocument.DocumentId.Value);
                        if (document != null) //If document already exists for this DefaultTemplate
                        {
                            await _storageManager.RemoveAsync(document);
                        }                    
                    }                    

                    request.uploadedFile.CopyTo(ms);
                    ms.Position = 0;
                    var doc = await _storageManager.StoreAsync(ms, request.uploadedFile.FileName, request.uploadedFile.ContentType);

                    if (defaultTemplateDocument is null)
                    {
                        defaultTemplateDocument = new DefaultTemplateDocument(defaultTemplate, doc.DocumentId);
                        await _repository.AddDefaultTemplateDocumentAsync(defaultTemplateDocument);
                        output = defaultTemplateDocument.DocumentId.Value;
                    }
                    else
                    {
                        defaultTemplateDocument.DocumentId = doc.DocumentId;
                        await _repository.UpdateDefaultTemplateDocumentAsync(defaultTemplateDocument);
                        output = defaultTemplateDocument.DocumentId.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Failure<Guid>(ex.Message);
            }

            return Result.Success(output);
        }
    }
}