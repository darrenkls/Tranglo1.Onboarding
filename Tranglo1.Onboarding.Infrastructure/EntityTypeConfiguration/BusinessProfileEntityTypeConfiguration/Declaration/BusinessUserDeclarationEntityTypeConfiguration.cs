using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Declaration;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class BusinessUserDeclarationEntityTypeConfiguration : BaseEntityTypeConfiguration<BusinessUserDeclaration>
    {
        protected override void Configure(EntityTypeBuilder<BusinessUserDeclaration> builder)
        {
            builder.ToTable("BusinessUserDeclarations", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("BusinessUserDeclarations");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("BusinessUserDeclarationCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(x => x.BusinessProfileCode)
                .IsRequired(true);
            


            builder.Property(o => o.Designation)
             .HasMaxLength(150);

            builder.Property(o => o.SigneeName)
            .HasMaxLength(150);

            builder.Property(o => o.BusinessUserDeclarationConcurrencyToken).IsConcurrencyToken();

        }
    }
}
