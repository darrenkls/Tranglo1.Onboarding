using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AMLCFTQuestionnaire: Entity
    {
        public Question Question { get; set; }
        public BusinessProfile BusinessProfile { get; set; }

        private AMLCFTQuestionnaire()
        {

        }

        public AMLCFTQuestionnaire(Question question, BusinessProfile businessProfile)
        {
            Question = question;
            BusinessProfile = businessProfile;
        }
    }
}

