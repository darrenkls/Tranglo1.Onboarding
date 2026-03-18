using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration
{
    public class DeclarationQuestionType : Enumeration
    {
		public DeclarationQuestionType() : base()
		{

		}
		public DeclarationQuestionType(int id, string name)
			: base(id, name)
		{

		}

		public static readonly DeclarationQuestionType Checkbox = new DeclarationQuestionType(1, "Checkbox");
		public static readonly DeclarationQuestionType Toggle = new DeclarationQuestionType(2, "Toggle");
		public static readonly DeclarationQuestionType Title = new DeclarationQuestionType(3, "Title");
	}
}