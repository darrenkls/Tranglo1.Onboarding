using System;

namespace Tranglo1.Onboarding.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string Type { get; set; }
        public string Detail { get; set; }
        public string Title { get; set; }
        //public string Instance { get; set; }
        public NotFoundException() : base()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            Title = "Not Found";
        }
    }
}
