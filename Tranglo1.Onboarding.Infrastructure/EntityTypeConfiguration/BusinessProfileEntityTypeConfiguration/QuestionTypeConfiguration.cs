using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class QuestionEntityTypeConfiguration : BaseEntityTypeConfiguration<Question>
    {
        protected override void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("QuestionCode");

            builder.Property(o => o.Description)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("QuestionDescription");

            builder.HasOne(o => o.QuestionInputType)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey(q => q.QuestionInputTypeCode);

            builder.HasOne(o => o.QuestionSection)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("QuestionSectionCode");

            builder.HasOne(o => o.ParentAnswerChoice)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("ParentAnswerChoiceCode");

            builder.HasOne(o => o.ParentQuestionCode)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("ParentQuestionCodeCode");

            builder.Property(o => o.SequenceNo)
                .HasDefaultValue(0);
        }
    }
}
