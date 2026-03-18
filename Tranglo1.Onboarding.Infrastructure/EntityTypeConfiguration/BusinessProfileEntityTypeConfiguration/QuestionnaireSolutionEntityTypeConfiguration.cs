using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class QuestionnaireSolutionEntityTypeConfiguration : BaseEntityTypeConfiguration<QuestionnaireSolution>
    {
        protected override void Configure(EntityTypeBuilder<QuestionnaireSolution> builder)
        {
            builder.ToTable("QuestionnaireSolutions", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("QuestionnaireSolutions");

                builder.HasKey(o => o.Id);

                builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("QuestionnaireSolutionCode");

                builder.HasOne(o => o.Questionnaire)
                .WithMany()
                .HasForeignKey("QuestionnaireCode")
                .IsRequired(true);

                builder.HasOne(o => o.Solution)
                .WithMany()
                .HasForeignKey("SolutionCode")
                .IsRequired(true);
            });
        }
    }
}
