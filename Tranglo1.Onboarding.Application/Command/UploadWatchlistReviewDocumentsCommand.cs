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
using Tranglo1.DocumentStorage.Models;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.Compliance, UACAction.Edit)]
    internal class UploadWatchlistReviewDocumentsCommand : BaseCommand<Result>
    {
        public long WatchlistReviewCode { get; set; }
        public IFormFile[] uploadedFiles { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Upload document(s) for watchlist review code : [{this.WatchlistReviewCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UploadWatchlistReviewDocumentsCommandHandler : IRequestHandler<UploadWatchlistReviewDocumentsCommand, Result>
    {
        private readonly IScreeningRepository _screeningRepository;
        private readonly ILogger<UploadWatchlistReviewDocumentsCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        public UploadWatchlistReviewDocumentsCommandHandler(IScreeningRepository screeningRepository,
                                                      ILogger<UploadWatchlistReviewDocumentsCommandHandler> logger,
                                                      StorageManager storageManager)
        {
            _screeningRepository = screeningRepository;
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<Result> Handle(UploadWatchlistReviewDocumentsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var watchlistReview = await _screeningRepository.GetWatchlistReviewById(request.WatchlistReviewCode);

                if (watchlistReview == null)
                {
                    return Result.Failure(
                        $"Watchlist Review Code is not valid: {request.WatchlistReviewCode}."
                        );
                }

                var isSuccess = true;
                foreach (var uploadedFile in request.uploadedFiles) 
                {
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            var extension = Path.GetExtension(uploadedFile.FileName);
                            var allowedExtensions = new[] { ".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".jpg", ".jpeg" };

                            var fileSize = uploadedFile.Length;
                            int maxFileSizeMB = 30;
                            var maxFileSize = maxFileSizeMB * 1024 * 1024;

                            if (fileSize > maxFileSize)
                            {
                                return Result.Failure($"Document file size exceeds {maxFileSizeMB}mb");
                            }
                            if (!allowedExtensions.Contains(extension))
                            {
                                return Result.Failure("Document extension is not allowed");
                            }

                            await uploadedFile.CopyToAsync(ms);
                            ms.Position = 0;
                            var doc = await _storageManager.StoreAsync(ms, uploadedFile.FileName, uploadedFile.ContentType);
                            if (doc != null)
                            {
                                WatchlistReviewDocument watchlistReviewDocument
                                        = new WatchlistReviewDocument(watchlistReview, doc.DocumentId);

                                await _screeningRepository.AddWatchlistReviewDocument(watchlistReviewDocument, cancellationToken);

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        isSuccess = false;
                    }
                }
                if (isSuccess)
                {
                    return Result.Success();
                }
                else
                {
                    Result.Failure("Unable to upload watchlist review document(s).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UploadWatchlistReviewDocumentsCommand] {ex.Message}");
            }

            return Result.Failure("Unable to upload watchlist review document(s).");

        }
    }
}
