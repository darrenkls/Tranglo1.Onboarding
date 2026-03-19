using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DefaultTemplateDocument : Entity
    {
        public DefaultTemplate DefaultTemplate { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
    }
}
