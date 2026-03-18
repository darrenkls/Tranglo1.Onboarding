using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.CustomerVerification;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.DocumentStorage;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetCustomerVerificationDocumentThumbnailDetailsQuery : BaseQuery<GetCustomerVerificationDocumentThumbnailDetailsOutputDTO>
    {
        public Guid DocumentId { get; set; }

        public override Task<string> GetAuditLogAsync(GetCustomerVerificationDocumentThumbnailDetailsOutputDTO result)
        {
            string _description = $"Get Document Details for Document ID: [{this.DocumentId}]";
            return Task.FromResult(_description);
        }
    }

    internal class GetCustomerVerificationDocumentThumbnailDetailsQueryHandler : IRequestHandler<GetCustomerVerificationDocumentThumbnailDetailsQuery, GetCustomerVerificationDocumentThumbnailDetailsOutputDTO>
    {
        private readonly ILogger<GetCustomerVerificationDocumentThumbnailDetailsQueryHandler> _logger;
        private readonly StorageManager _storageManager;

        public GetCustomerVerificationDocumentThumbnailDetailsQueryHandler(ILogger<GetCustomerVerificationDocumentThumbnailDetailsQueryHandler> logger, StorageManager storageManager)
        {
            _logger = logger;
            _storageManager = storageManager;
        }

        public async Task<GetCustomerVerificationDocumentThumbnailDetailsOutputDTO> Handle(GetCustomerVerificationDocumentThumbnailDetailsQuery request, CancellationToken cancellationToken)
        {
            var document = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

            if (document != null)
            {
                var ms = new MemoryStream();
                await _storageManager.CopyToAsync(document.DocumentId, ms);
                // Check if any data was copied to the MemoryStream
                if (ms.Length > 0)
                {
                    var rawImageData = ms.ToArray();
                    var compressedImageData = CompressImage(rawImageData, maxWidth: 250, maxHeight: 300);

                    return new GetCustomerVerificationDocumentThumbnailDetailsOutputDTO()
                    {
                        FileData = compressedImageData,
                        ContentType = document.ContentType,
                        FileName = document.FileName,
                        FileSize = compressedImageData.Length
                    };
                }
                else
                {
                    return Result.Failure<GetCustomerVerificationDocumentThumbnailDetailsOutputDTO>("Failed to retrieve document data.").Value;                
                }
            }

            return null;
        }

        private byte[] CompressImage(byte[] imageData, int maxWidth, int maxHeight)
        {
            using (var rawImageStream = new MemoryStream(imageData))
            {
                using (var rawImage = Image.FromStream(rawImageStream))
                {
                    // Calculate new dimensions while maintaining the aspect ratio
                    int newWidth, newHeight;
                    CalculateAspectRatio(rawImage.Width, rawImage.Height, maxWidth, maxHeight, out newWidth, out newHeight);

                    using (var compressedImage = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(compressedImage))
                        {
                            // Perform image compression by drawing the raw image onto the compressed image
                            graphics.DrawImage(rawImage, 0, 0, newWidth, newHeight);
                        }

                        using (var compressedImageStream = new MemoryStream())
                        {
                            // Save the compressed image to a new MemoryStream
                            compressedImage.Save(compressedImageStream, rawImage.RawFormat);

                            // Get the compressed image data from the MemoryStream
                            return compressedImageStream.ToArray();
                        }
                    }
                }
            }
        }

        private void CalculateAspectRatio(int originalWidth, int originalHeight, int maxWidth, int maxHeight, out int newWidth, out int newHeight)
        {
            double aspectRatio = (double)originalWidth / originalHeight;

            if (originalWidth > maxWidth || originalHeight > maxHeight)
            {
                // If either dimension exceeds the max, calculate the new dimensions while maintaining the aspect ratio
                if (aspectRatio > 1)
                {
                    // Landscape image
                    newWidth = maxWidth;
                    newHeight = (int)(maxWidth / aspectRatio);
                }
                else
                {
                    // Portrait or square image
                    newWidth = (int)(maxHeight * aspectRatio);
                    newHeight = maxHeight;
                }
            }
            else
            {
                // If neither dimension exceeds the max, keep the original dimensions
                newWidth = originalWidth;
                newHeight = originalHeight;
            }
        }
    }
}
