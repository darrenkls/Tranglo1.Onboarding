using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class COInformationEntityTypeConfiguration : BaseEntityTypeConfiguration<COInformation>
    {
        protected override void Configure(EntityTypeBuilder<COInformation> builder)
        {
            builder.ToTable("COInformations", BusinessProfileDbContext.DEFAULT_SCHEMA);

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("COInformationCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.ComplianceOfficer)
                    .HasMaxLength(150);

            builder.Property(e => e.PositionTitle)
                    .HasMaxLength(150);

            builder.Property(e => e.CompanyAddress)
                    .HasMaxLength(150);

            builder.Property(e => e.ZipCodePostCode)
                    .HasMaxLength(15);

            builder.OwnsOne<ContactNumber>(e => e.ContactNumber, contactNumber =>
            {
                contactNumber.Property(e => e.Value)
                    .HasColumnName("ContactNumber")
                    .HasMaxLength(50);

                contactNumber.Property(e => e.DialCode)
                    .HasColumnName("DialCode")
                    .HasMaxLength(50);

                contactNumber.Property(e => e.CountryISO2Code)
                    .HasColumnName("ContactNumberCountryISO2")
                    .HasMaxLength(2);

                contactNumber.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            });

            builder.OwnsOne<Email>(e => e.EmailAddress, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("EmailAddress")
                    .HasMaxLength(150);

                email.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            });
            //builder.Property(e => e.EmailAddress)
            //        .HasMaxLength(150);

            builder.Property(e => e.ReportingTo)
                    .HasMaxLength(150);

            builder.Property(e => e.CertificationProgram)
                    .HasMaxLength(150);

            builder.Property(e => e.CertificationBodyOrganization)
                    .HasMaxLength(150);

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");

        }
    }
}
