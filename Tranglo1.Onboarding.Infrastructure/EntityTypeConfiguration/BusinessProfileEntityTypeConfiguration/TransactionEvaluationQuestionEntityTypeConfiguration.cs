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
    class TransactionEvaluationQuestionEntityTypeConfiguration : BaseEntityTypeConfiguration<TransactionEvaluationQuestion>
    {
        protected override void Configure(EntityTypeBuilder<TransactionEvaluationQuestion> builder)
        {
            builder.ToTable("TransactionEvaluationQuestions", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("TransactionEvaluationQuestionCode");

            builder.HasOne(o => o.TransactionEvaluationQuestionInputType)
               .WithMany()
               .HasForeignKey("TransactionEvaluationQuestionInputTypeCode");

            builder.Property(o => o.TransactionEvaluationQuestionDescription)
                .HasMaxLength(500);

            //TransactionEvaluationQuestionInputType

        }
    }
}
