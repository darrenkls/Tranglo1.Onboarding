using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Verification;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.CustomerUserVerification
{
    class CustomerVerificationEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerVerification>
    {
        protected override void Configure(EntityTypeBuilder<CustomerVerification> builder)
        {
            builder.ToTable("CustomerVerifications", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CustomerVerifications");
            });

            //Primary Key 
            builder.Property(a => a.Id)
                .HasColumnName("CustomerVerificationCode");
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

            builder.HasOne(a => a.EKYCVerificationStatus)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("EKYCVerificationStatusCode");

            builder.HasOne(a => a.F2FVerificationStatus)
              .WithMany()
              .IsRequired(false)
              .HasForeignKey("F2FVerificationStatusCode");


            builder.HasOne(a => a.VerificationIDType)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("VerificationIDTypeCode");


            builder.Property(a => a.JustificationRemark)
                   .HasMaxLength(500);


            builder.HasOne(a => a.RiskScore)
             .WithMany()
           .IsRequired(false)
             .HasForeignKey("RiskScoreCode");


            builder.HasOne(a => a.RiskType)
             .WithMany()
           .IsRequired(false)
             .HasForeignKey("RiskTypeCode");

            builder.Property(a => a.TemplateID)
             .HasColumnName("TemplateID");

        }
    }
}
