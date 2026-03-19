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
    class CustomerBusinessTransactionEvaluationAnswerEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerBusinessTransactionEvaluationAnswer>
    {
        protected override void Configure(EntityTypeBuilder<CustomerBusinessTransactionEvaluationAnswer> builder)
        {
            builder.ToTable("CustomerBusinessTransactionEvaluationAnswers", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CustomerBusinessTransactionEvaluationAnswers");
            });


            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("CustomerBusinessTransactionEvaluationAnswerCode");

            builder.HasOne(o => o.TransactionEvaluationQuestion)
               .WithMany()
               .HasForeignKey("TransactionEvaluationQuestionCode");

            builder.HasOne(o => o.TransactionEvaluationAnswerChoice)
               .WithMany()
               .HasForeignKey("TransactionEvaluationAnswerChoiceCode");

        }
    }
}
