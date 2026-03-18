using System.IO;

namespace Tranglo1.Onboarding.Application.DTO
{
    public class FileOutputDTO
    {
        public Stream File { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string? FilePath { get; set; }
    }
}
