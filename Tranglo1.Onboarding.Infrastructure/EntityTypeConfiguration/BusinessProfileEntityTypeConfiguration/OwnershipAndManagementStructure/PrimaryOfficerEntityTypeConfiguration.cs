using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class PrimaryOfficerEntityTypeConfiguration : BaseEntityTypeConfiguration<PrimaryOfficer>
    {
        protected override void Configure(EntityTypeBuilder<PrimaryOfficer> builder)
        {
            builder.ToTable("PrimaryOfficers", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("PrimaryOfficers");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("PrimaryOfficerCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.Name)
                    .HasMaxLength(150);

            builder.Property(e => e.PositionTitle)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Gender)
                .WithMany()
                .HasForeignKey("GenderCode");

            builder.HasOne(e => e.Nationality)
              .WithMany()
              .HasForeignKey("CountryCode")
              .IsRequired(false);

            builder.HasOne(e => e.CountryOfResidence)
            .WithMany()
            .HasForeignKey("CountryResidenceCode")
            .IsRequired(false);

            builder.HasOne(o => o.IDType)
                .WithMany()
                .HasForeignKey("IDTypeCode");

            builder.Property(e => e.IDNumber)
                    .HasMaxLength(150);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

            builder.HasOne(o => o.Shareholder)
               .WithMany()
               .IsRequired(false)
               .HasForeignKey("ShareholderCode");

            builder.Property(e => e.ResidentialAddress)
                .HasMaxLength(300);

            builder.Property(e => e.ZipCodePostCode)
                .HasMaxLength(150);

            builder.HasOne(o => o.Title)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("TitleCode");

            builder.Property(o => o.TitleOthers)
                .HasMaxLength(50);

        }
    }
}
