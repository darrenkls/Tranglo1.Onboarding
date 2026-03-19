using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    
    class IndividualShareholderEntityTypeConfiguration : BaseEntityTypeConfiguration<IndividualShareholder>
    {

        protected override void Configure(EntityTypeBuilder<IndividualShareholder> builder)
        {
            builder.ToTable("IndividualShareholders", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("IndividualShareholders");
            });


            builder.Property(e => e.Name)
                    .HasMaxLength(150);

            builder.Property(e => e.EffectiveShareholding)
                    .HasMaxLength(150);

            builder.HasOne(e => e.Nationality)
             .WithMany()
             .HasForeignKey("CountryCode")
             .IsRequired(false);

            builder.HasOne(e => e.CountryOfResidence)
            .WithMany()
            .HasForeignKey("CountryResidenceCode")
            .IsRequired(false);

            builder.HasOne(e => e.IDType)
                    .WithMany()
                    .HasForeignKey("IDTypeCode");

            builder.Property(e => e.IDNumber)
                    .HasMaxLength(150);

            builder.HasOne(o => o.Gender)
                .WithMany()
                .HasForeignKey("GenderCode");

            builder.Property(a => a.ResidentialAddress)
                .HasMaxLength(300);

            builder.Property(a => a.ZipCodePostCode)
                .HasMaxLength(150);

            builder.Property(a => a.PositionTitle)
                .HasMaxLength(50);

            builder.HasOne(o => o.Title)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("TitleCode");

            builder.Property(o => o.TitleOthers)
                .HasMaxLength(50);
        }
    }

    
}
