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
 
    class ShareholderEntityTypeConfiguration : BaseEntityTypeConfiguration<Shareholder>
    {
        protected override void Configure(EntityTypeBuilder<Shareholder> builder)
        {
            builder.ToTable("Shareholders", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("Shareholders");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("ShareholderCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.EffectiveShareholding)
                    .HasMaxLength(150);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

            builder.HasOne(o => o.BoardOfDirector)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("BoardOfDirectorCode");

            builder.HasOne(o => o.PrimaryOfficer)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("PrimaryOfficerCode");

            builder.HasOne(o => o.AuthorisedPerson)
               .WithMany()
               .IsRequired(false)
               .HasForeignKey("AuthorisedPersonCode");

            builder.HasOne(o => o.UltimateBeneficialOwner)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("UltimateBeneficialOwnerCode");

        }
    }
 
}
