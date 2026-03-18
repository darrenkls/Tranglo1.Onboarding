using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class AffiliateAndSubsidiaryEntityTypeConfiguration : BaseEntityTypeConfiguration<AffiliateAndSubsidiary>
    {
        protected override void Configure(EntityTypeBuilder<AffiliateAndSubsidiary> builder)
        {
            builder.ToTable("AffiliatesAndSubsidiaries", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("AffiliatesAndSubsidiaries");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("AffiliateAndSubsidiaryCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.CompanyName)
                    .HasMaxLength(150);

            builder.Property(e => e.CompanyRegNo)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Country)
                .WithMany()
                .HasForeignKey("CountryCode")
                .IsRequired(false);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

        }
    }
}
