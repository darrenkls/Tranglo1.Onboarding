using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using MimeDetective;
using MimeDetective.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tranglo1.Onboarding.Application.Helper
{
    public static class FileHelper
    {
        private static readonly IContentInspector _inspector = new ContentInspectorBuilder()
        {
            Definitions = DefaultDefinitions.All()
        }.Build();
        // Max number of bytes to read for validation (helps avoid huge/malicious files)
        private const int MaxScanBytes = 4096;
        private static readonly HashSet<string> AcceptedFileTypes = new HashSet<string>
        {
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".zip",
            ".pdf",
            ".png",
            ".jpg",
            ".jpeg",
            ".csv"
        };
        private static readonly HashSet<string> BlockedFileTypes = new HashSet<string>
        {
            ".exe",
            ".com",
            ".bat",
            ".cmd",
            ".scr",
            ".cpl",
            ".msi",
            ".app",
            ".osx",
            ".ipa",
            ".sh",
            ".bin",
            ".run",
            ".elf",
            ".apk"
        };

        public static Result<bool> ValidateFileHasTrueExtension(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            string fileContentErrorMessage = "File content does not match its extension. " +
                "Detected: {0}, Expected: {1}";
            string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            using var stream = file.OpenReadStream();
            var match = _inspector.Inspect(stream)
                .OrderByDescending(x => x.Points)
                .FirstOrDefault();

            stream.Position = 0;

            if (match == null)
            {
                #region Manual CSV file validation (fallback) due to cannot detect file signatures by Mime-Detective
                if (IsCSV(file.FileName, stream))
                {
                    return true;
                }

                #endregion

                return Result.Failure<bool>(String.Format(fileContentErrorMessage, "Unknown", extension));
            }

            var extensions = match.Definition.File.Extensions;
            if (!extensions.Any(x => ExtensionMatches(extension, x)))
            {
                string detectedExt = extensions.FirstOrDefault();
                detectedExt = detectedExt.StartsWith(".")
                    ? detectedExt
                    : $".{detectedExt}";

                #region Manual CSV file validation (fallback) due to cannot detect file signatures by Mime-Detective
                if (IsCSV(file.FileName, stream))
                {
                    return true;
                }
                #endregion

                return Result.Failure<bool>(String.Format(fileContentErrorMessage, detectedExt, extension));
            }

            return true;
        }

        public static Result<bool> ValidateFileHasSingleExtension(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            string[] parts = file.FileName.Split(".");

            // Remove the first array element (the filename without extension)
            if (parts.Length > 1)
            {
                parts = parts.Skip(1).ToArray();
            }

            // Take only the last 2 elements from the remaining parts
            if (parts.Length > 2)
            {
                parts = parts.TakeLast(2).ToArray();
            }

            if (parts.Length <= 1)
                return true;

            int extensionCount = 0;
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                string ext = "." + parts[i].ToLowerInvariant();

                if (AcceptedFileTypes.Contains(ext) || BlockedFileTypes.Contains(ext))
                    extensionCount++;
                else
                    break;      // Exit if the consecutive part isn't a valid extension
            }

            if (extensionCount > 1)
            {
                return Result.Failure<bool>("The file has double extension. Please rename it and upload again.");
            }

            return true;
        }

        public static Result<bool> ValidateFileHasAcceptedExtension(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (BlockedFileTypes.Contains(extension) || !AcceptedFileTypes.Contains(extension))
            {
                return Result.Failure<bool>("This file format isn’t supported. Please upload a valid document type.");
            }

            return true;
        }

        private static bool ExtensionMatches(string fileNameExtension, string detectedExtension)
        {
            string normalize(string ext) => ext?.TrimStart('.').ToLowerInvariant() ?? "";

            return String.Equals(normalize(fileNameExtension), normalize(detectedExtension), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCSV(string fileName, Stream stream)
        {
            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            stream.Position = 0;
            ms.Position = 0;

            byte[] fileBytes = ms.ToArray();

            return IsCSV(fileName, fileBytes);
        }

        private static bool IsCSV(string fileName, byte[] fileBytes)
        {
            // Check extension
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return false;

            // Scan only the first part of the file
            int length = Math.Min(fileBytes.Length, MaxScanBytes);
            string textSample;

            try
            {
                textSample = Encoding.UTF8.GetString(fileBytes, 0, length);
            }
            catch
            {
                // Not a CSV if it can't be decoded as text
                return false;
            }

            // Reject files containing null bytes (binary indicator)
            if (textSample.Contains('\0'))
                return false;

            return true;
        }
    }
}
