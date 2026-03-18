using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.AMLCFTRelationshipTieUpEntityTypeConfiguration
{
    class AMLCFTDisplayRuleEntityTypeConfiguration : BaseEntityTypeConfiguration<AMLCFTDisplayRules>
    {
        protected override void Configure(EntityTypeBuilder<AMLCFTDisplayRules> builder)
        {
            builder.ToTable("AMLCFTDisplayRules", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("AMLCFTDisplayRules");
            });

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("AMLCFTDisplayRuleCode");

            builder.HasOne(o => o.EntityType)
              .WithMany()
              .HasForeignKey("EntityTypeCode");

            builder.HasOne(o => o.RelationshipTieUp)
              .WithMany()
              .HasForeignKey("RelationshipTieUpCode");

            builder.HasOne(o => o.Questionnaire)
              .WithMany()
              .HasForeignKey("QuestionnaireCode");

            builder.HasOne(o => o.ServicesOffered)
              .WithMany()
              .HasForeignKey("ServicesOfferedCode"); 
        }
    }
}
