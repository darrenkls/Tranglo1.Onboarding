using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class AMLCFTDisplayRules : Entity
    {
        public EntityType EntityType { get; set; }
        public RelationshipTieUp RelationshipTieUp { get; set; }
        public Questionnaire Questionnaire { get; set; }
        public bool IsIntermediary { get; set; }
        public ServicesOffered ServicesOffered { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get;set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public AMLCFTDisplayRules()
        {

        }
        public AMLCFTDisplayRules(EntityType entityType, RelationshipTieUp tieUp, ServicesOffered servicesOffered, Questionnaire questionnaire)
        {
            this.EntityType = entityType;
            this.RelationshipTieUp = tieUp;
            this.ServicesOffered = servicesOffered;
            this.Questionnaire = questionnaire;
        }
    }
}
