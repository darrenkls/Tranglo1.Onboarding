using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.CustomerUserVerification;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.CustomerUserVerification
{
    class CustomerVerificationDocumentsEntityTypeConfiguration : BaseEntityTypeConfiguration<CustomerVerificationDocuments>
    {
        protected override void Configure(EntityTypeBuilder<CustomerVerificationDocuments> builder)
        {
            builder.ToTable("CustomerVerificationDocuments", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("CustomerVerificationDocuments");
            });



            //Primary Key 
            builder.Property(a => a.Id)
                .HasColumnName("CustomerVerificationDocumentCode");
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.CustomerVerification)
            .WithMany()
            .IsRequired(false)
            .HasForeignKey("CustomerVerificationCode");


            builder.Property(a => a.RawDocumentID)
             .HasColumnName("RawDocumentID");

            builder.Property(a => a.WatermarkDocumentID)
            .HasColumnName("WatermarkDocumentID");


            builder.HasOne(a => a.VerificationIDTypeSection)
            .WithMany()
           .IsRequired(false)
            .HasForeignKey("VerificationIDTypeSectionCode");

            builder.HasOne(a => a.SubmissionResult)
           .WithMany()
           .IsRequired(false)
           .HasForeignKey("SubmissionResultCode");


        }
    }
}
