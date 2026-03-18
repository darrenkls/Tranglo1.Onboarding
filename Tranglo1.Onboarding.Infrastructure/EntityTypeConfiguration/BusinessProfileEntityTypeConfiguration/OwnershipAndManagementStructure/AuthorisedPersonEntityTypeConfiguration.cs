using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class AuthorisedPersonEntityTypeConfiguration : BaseEntityTypeConfiguration<AuthorisedPerson>
    {
        protected override void Configure(EntityTypeBuilder<AuthorisedPerson> builder)
        {
            builder.ToTable("AuthorisedPersons", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("AuthorisedPersons");
            });

            //Primary Key
            builder.Property(o => o.Id)
                .HasColumnName("AuthorisedPersonCode");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.FullName)
                .HasMaxLength(150);

            builder.HasOne(o => o.AuthorisationLevel)
                .WithMany()
                .HasForeignKey("AuthorisationLevelCode");

            builder.HasOne(e => e.Nationality)
               .WithMany()
               .HasForeignKey("NationalityCode")
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

            builder.Property(o => o.PositionTitle)
                .HasMaxLength(50);

            builder.HasOne(o => o.BusinessProfile)
             .WithMany()
             .IsRequired(true)
             .HasForeignKey("BusinessProfileCode");

            builder.HasOne(o => o.Shareholder)
            .WithMany()
            .IsRequired(false)
            .HasForeignKey("ShareholderCode");

            builder.OwnsOne<ContactNumber>(user => user.TelephoneNumber, kyc =>
            {
                kyc.Property(e => e.Value)
                    .HasColumnName("TelephoneNumber")
                    .HasMaxLength(50);

                kyc.Property(e => e.DialCode)
                    .HasColumnName("TelephoneDialCode")
                    .HasMaxLength(50);

                kyc.Property(e => e.CountryISO2Code)
                    .HasColumnName("TelephoneNumberCountryISO2Code")
                    .HasMaxLength(2);

                kyc.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            });

            builder.Property(e => e.EmailAddress)
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
