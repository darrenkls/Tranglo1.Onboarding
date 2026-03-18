using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
     class QuestionManagementEntityTypeConfiguration : BaseEntityTypeConfiguration<QuestionManagement>
    {
        protected override void Configure(EntityTypeBuilder<QuestionManagement> builder)
        {
            builder.ToTable("QuestionManagements", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("QuestionManagements");
            });

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("QuestionManagementCode");

            builder.HasOne(o => o.Questionnaire)
                .WithMany()
                .HasForeignKey("QuestionnaireCode")
                .IsRequired(false);

            builder.HasOne(o => o.Solution)
                .WithMany()
                .HasForeignKey("SolutionCode")
                .IsRequired(false);

            builder.HasOne(o => o.TrangloEntity)
                .WithMany()
                .HasForeignKey("TrangloEntityCode")
                .IsRequired(false);

            builder.Property(o => o.IsChecked)
                .IsRequired(false)
                .HasColumnName("IsChecked");
        }
    }
}
