using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class LicenseInformationEntityTypeConfiguration : BaseEntityTypeConfiguration<LicenseInformation>
    {
        protected override void Configure(EntityTypeBuilder<LicenseInformation> builder)
        {
            builder.ToTable("LicenseInformations", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("LicenseInformations");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("LicenseInformationCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.IsLicenseRequired)
                    .IsRequired(false);

            builder.Property(e => e.LicenseType)
                    .HasMaxLength(150);

            builder.Property(e => e.LicenseCertNumber)
                    .HasMaxLength(150);

            builder.Property(e => e.PrimaryRegulatorLicenseService)
                    .HasMaxLength(150);

            builder.Property(e => e.PrimaryRegulatorAMLCFT)
                    .HasMaxLength(150);

            builder.Property(e => e.ActLawRemittanceLicense)
                    .HasMaxLength(500);

            builder.Property(e => e.ActLawAMLCFT)
                    .HasMaxLength(500);

            builder.Property(e => e.Remark)
           .HasMaxLength(2000);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

        }
    }
}
