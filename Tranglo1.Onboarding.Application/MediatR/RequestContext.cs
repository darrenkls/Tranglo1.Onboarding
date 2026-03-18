using System.Threading;

namespace Tranglo1.Onboarding.Application.MediatR
{
    public static class RequestContext
    {
        private static readonly AsyncLocal<bool> _isUacRequest = new AsyncLocal<bool>();

        public static bool IsUacRequest
        {
            get => _isUacRequest.Value;
            set => _isUacRequest.Value = value;
        }
    }
}
