using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class PoliticallyExposedPersonEntityTypeConfiguration : BaseEntityTypeConfiguration<PoliticallyExposedPerson>
    {
        protected override void Configure(EntityTypeBuilder<PoliticallyExposedPerson> builder)
        {
            builder.ToTable("PoliticallyExposedPersons", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("PoliticallyExposedPersons");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("PoliticallyExposedPersonCode");
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
              .HasForeignKey("CountryISO2")
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

        }
    }
}
