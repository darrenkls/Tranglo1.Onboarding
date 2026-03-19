using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class CustomerUserBusinessProfileEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerUserBusinessProfile>
    {
        protected override void Configure(EntityTypeBuilder<CustomerUserBusinessProfile> builder)
        {
            builder.ToTable("CustomerUserBusinessProfiles", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CustomerUserBusinessProfiles");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("CustomerUserBusinessProfileCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.CustomerUser)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("UserId");

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");
            /*
            builder.HasOne(o => o.AccountStatus)
            .WithMany()
            .HasForeignKey("AccountStatusCode")
            .IsRequired(true);
            */
            builder.HasOne(o => o.Environment)
                .WithMany()
                .HasForeignKey("EnvironmentCode")
                .IsRequired(false);

        }
    }
}