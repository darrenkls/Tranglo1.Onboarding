using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class BoardOfDirectorEntityTypeConfiguration : BaseEntityTypeConfiguration<BoardOfDirector>
    {
        protected override void Configure(EntityTypeBuilder<BoardOfDirector> builder)
        {
            builder.ToTable("BoardOfDirectors", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("BoardOfDirectors");
            });

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("BoardOfDirectorCode");
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

            builder.Property(o => o.ResidentialAddress)
             .HasMaxLength(300);

            builder.Property(o => o.ZipCodePostCode)
                .HasMaxLength(150);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

            builder.HasOne(o => o.Shareholder)
               .WithMany()
               .IsRequired(false)
               .HasForeignKey("ShareholderCode");

            builder.HasOne(o => o.Title)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("TitleCode");

            builder.Property(o => o.TitleOthers)
                .HasMaxLength(50);

        }
    }
}
