using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.DeclarationQuestionTypeEntityTypeConfiguration
{
    class DeclarationQuestionTypeEntityTypeConfiguration : BaseEntityTypeConfiguration<DeclarationQuestionType>
    {
        protected override void Configure(EntityTypeBuilder<DeclarationQuestionType> builder)
        {
            builder.ToTable("DeclarationQuestionTypes", BusinessProfileDbContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("DeclarationQuestionTypeCode");

            builder.Property(o => o.Name)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<DeclarationQuestionType>());
        }
    }
}