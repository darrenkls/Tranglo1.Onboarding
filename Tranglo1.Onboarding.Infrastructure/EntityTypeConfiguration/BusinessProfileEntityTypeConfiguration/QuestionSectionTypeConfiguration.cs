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
    class QuestionSectionEntityTypeConfiguration : BaseEntityTypeConfiguration<QuestionSection>
    {
        protected override void Configure(EntityTypeBuilder<QuestionSection> builder)
        {
            builder.ToTable("QuestionSections", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("QuestionSections");
            });


            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("QuestionSectionCode");

            builder.Property(o => o.Description)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("QuestionSectionDescription");

            builder.HasOne(o => o.Questionnaire)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("QuestionnaireCode");
        }
    }
}
