using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Questionnaire : Entity
    {
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsBank { get; set; }
        public bool IsIntermediaryRemittance { get; set; }

        private Questionnaire()
        {

        }

        public Questionnaire(string description, bool isActive)
        {
            Description = description;
            IsActive = isActive;
            IsBank = false;
            IsIntermediaryRemittance = false;
        }

        public Questionnaire(long id, string description)
        {
            Id = id;
            Description = description;
            IsActive = true;
            IsBank = false;
            IsIntermediaryRemittance = false;
        }
    }
}
