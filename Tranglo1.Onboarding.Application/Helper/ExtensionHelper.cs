using System.Runtime.CompilerServices;

namespace Tranglo1.Onboarding.Application.Helper
{
    public static class ExtensionHelper
    {
        public static string GetMethodName([CallerMemberName] string callerMemberName = "") => callerMemberName;
    }
}
