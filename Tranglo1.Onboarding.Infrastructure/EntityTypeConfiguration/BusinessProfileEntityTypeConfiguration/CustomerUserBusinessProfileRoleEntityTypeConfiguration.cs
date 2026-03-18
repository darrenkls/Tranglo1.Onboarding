using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class CustomerUserBusinessProfileRoleEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerUserBusinessProfileRole>
    { 
        protected override void Configure(EntityTypeBuilder<CustomerUserBusinessProfileRole> builder)
        {
            builder.ToTable("CustomerUserBusinessProfileRoles", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CustomerUserBusinessProfileRoles");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("CustomerUserBusinessProfileRoleCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.CustomerUserBusinessProfile)
                .WithMany()
                .HasForeignKey("CustomerUserBusinessProfileCode")
                .IsRequired(true);
            
            builder.HasOne(o => o.UserRole)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("UserRoleCode");

            builder.Property(o => o.RoleCode)
                .HasMaxLength(10)
                .IsRequired(false);

            /*
            builder.HasOne(o => o.Role)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("RoleCode");
            */

        }
    }
}