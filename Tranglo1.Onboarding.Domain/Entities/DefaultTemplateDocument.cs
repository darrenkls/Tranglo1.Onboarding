using CSharpFunctionalExtensions;
using System;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class DefaultTemplateDocument : Entity
    {
		public DefaultTemplate DefaultTemplate { get; set; }
		public Guid? DocumentId { get; set; }

		private DefaultTemplateDocument() { }

		public DefaultTemplateDocument(DefaultTemplate defaultTemplate, Guid? documentId)
		{
			this.DefaultTemplate = defaultTemplate;
			this.DocumentId = documentId;
		}
	}
}
