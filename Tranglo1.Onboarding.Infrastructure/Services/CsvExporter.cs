using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
    public class CsvExporter
    {
        public async Task<CsvExportResult> ExportAsync<T>(IEnumerable<T> records, string fileName)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }
            
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
            }

            var memoryStream = new MemoryStream();

            await using var writer = new StreamWriter(
                memoryStream, 
                Encoding.UTF8, 
                65536, // 64 KB buffer size
                leaveOpen: true
                );

            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
                await csv.FlushAsync();
            }

            memoryStream.Position = 0;

            return new CsvExportResult
            {
                Stream = memoryStream,
                FileName = fileName,
                ContentType = "text/csv"
            };
        }
    }

    public class CsvExportResult
    {
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
