using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.TransactionEvaluation;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class TransactionEvaluationAnswerChoiceEntityTypeConfiguration : BaseEntityTypeConfiguration<TransactionEvaluationAnswerChoice>
    {
        protected override void Configure(EntityTypeBuilder<TransactionEvaluationAnswerChoice> builder)
        {
            builder.ToTable("TransactionEvaluationAnswerChoices", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("TransactionEvaluationAnswerChoices");
            });

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("TransactionEvaluationAnswerChoiceCode");

            builder.HasOne(o => o.TransactionEvaluationQuestion)
               .WithMany()
               .HasForeignKey("TransactionEvaluationQuestionCode");

            builder.Property(o => o.AnswerChoiceDescription)
                 .HasMaxLength(500);

        }
    }
}
