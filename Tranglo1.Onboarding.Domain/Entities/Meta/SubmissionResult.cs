using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class SubmissionResult : Enumeration
    {
        public SubmissionResult() : base() { }

        public SubmissionResult(int id, string name) : base(id, name) { }
    }
}
