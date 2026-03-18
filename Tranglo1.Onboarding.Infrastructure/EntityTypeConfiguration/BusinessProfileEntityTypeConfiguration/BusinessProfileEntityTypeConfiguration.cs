using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class BusinessProfileEntityTypeConfiguration : BaseEntityTypeConfiguration<BusinessProfile>
    {
        protected override void Configure(EntityTypeBuilder<BusinessProfile> builder)
        {
            builder.ToTable("BusinessProfiles", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("BusinessProfiles");
            });

            //Primary Key
            builder.Property(complianceOfficerLoginId => complianceOfficerLoginId.Id)
                    .HasColumnName("ComplianceOfficerLoginId");
            builder.HasKey(complianceOfficerLoginId => complianceOfficerLoginId.Id);

            builder.Property(kyc => kyc.Id)
                    .HasColumnName("BusinessProfileCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(x => x.WorkflowStatus)
                .HasConversion(x => x.Id, x => Enumeration.FindById<WorkflowStatus>(x))
                .HasColumnName("WorkFlowStatusCode");

            //builder.HasOne(o => o.WorkflowStatus)
            //    .WithMany()
            //    .HasForeignKey("WorkFlowStatusCode")
            //    .IsRequired(false);

            builder.Property(x => x.KYCStatus)
                .HasConversion(x => x.Id, x => Enumeration.FindById<KYCStatus>(x))
                .HasColumnName("KYCStatusCode");

            //builder.HasOne(o => o.KYCStatus)
            //    .WithMany()
            //    .HasForeignKey("KYCStatusCode");

            builder.HasOne(o => o.KYCSubmissionStatus)
                .WithMany()
                .HasForeignKey("KYCSubmissionStatusCode");

            builder.HasOne(o => o.Solution)
                .WithMany()
                .HasForeignKey("SolutionCode")
                .IsRequired(false);

            builder.Property(e => e.CompanyName)
                    .HasMaxLength(150);

            builder.Property(e => e.CompanyRegistrationName)
                    .HasMaxLength(500);

            builder.Property(e => e.TradeName)
                    .HasMaxLength(150);

            builder.Property(e => e.CompanyRegisteredAddress)
                    .HasMaxLength(500);

            builder.Property(e => e.CompanyRegisteredZipCodePostCode)
                    .HasMaxLength(15);

            builder.Property(e => e.MailingAddress)
                    .HasMaxLength(500);

            builder.Property(e => e.MailingZipCodePostCode)
                    .HasMaxLength(15);

            builder.Property(e => e.BusinessNature)
                .HasConversion(e => e.Id, e => Enumeration.FindById<BusinessNature>(e))
                .HasColumnName("BusinessNatureCode");
            //builder.HasOne(o => o.BusinessNature)
            //    .WithMany()
            //    .HasForeignKey("BusinessNatureCode");

            builder.Property(e => e.CompanyRegistrationNo)
                    .HasMaxLength(150);

            builder.OwnsOne<ContactNumber>(user => user.ContactNumber, kyc =>
            {
                kyc.Property(e => e.Value)
                    .HasColumnName("ContactNumber")
                    .HasMaxLength(50);

                kyc.Property(e => e.DialCode)
                    .HasColumnName("DialCode")
                    .HasMaxLength(50);

                kyc.Property(e => e.CountryISO2Code)
                    .HasColumnName("ContactNumberCountryISO2Code")
                    .HasMaxLength(2);

                kyc.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            });

            builder.Property(e => e.Website)
                    .HasMaxLength(150);

            builder.Property(e => e.StockExchangeName)
                    .HasMaxLength(150);

            builder.Property(e => e.StockCode)
                    .HasMaxLength(150);

            builder.Property(e => e.OtherReason)
                    .HasMaxLength(150);

            /* builder.Property(e => e.Feedback)
                     .HasMaxLength(500);*/

            /*builder.HasOne(o => o.ReviewResult)
                .WithMany()
                .HasForeignKey("ReviewResultCode");*/


            //builder.Property(p => p.RowVersion)
            //        .IsConcurrencyToken();

            builder.Ignore(o => o.DomainEvents);

            builder.Property(o => o.ScreeningCode)
               .IsRequired(false)
               .HasColumnName("ScreeningCode");

            builder.HasOne(o => o.MailingCountryMeta)
                .WithMany()
                .HasForeignKey("MailingCountryCode");

            builder.HasOne(o => o.CompanyRegisteredCountryMeta)
                .WithMany()
                .HasForeignKey("CompanyRegisteredCountryCode");


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


            builder.OwnsOne<ContactNumber>(user => user.FacsimileNumber, kyc =>
            {
                kyc.Property(e => e.Value)
                    .HasColumnName("FacsimileNumber")
                    .HasMaxLength(50);

                kyc.Property(e => e.DialCode)
                    .HasColumnName("FacsimileNumberDialCode")
                    .HasMaxLength(50);

                kyc.Property(e => e.CountryISO2Code)
                    .HasColumnName("FacsimileNumberCountryISO2Code")
                    .HasMaxLength(2);

                kyc.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            });

            builder.HasOne(o => o.EntityType)
               .WithMany()
               .HasForeignKey("EntityTypeCode")
               .IsRequired(false);

            builder.HasOne(o => o.RelationshipTieUp)
               .WithMany()
               .HasForeignKey("RelationshipTieUpCode")
               .IsRequired(false);

            builder.HasOne(o => o.IncorporationCompanyType)
               .WithMany()
               .HasForeignKey("IncorporationCompanyTypeCode")
               .IsRequired(false);

            builder.Property(e => e.TaxIdentificationNo)
                .HasMaxLength(150);


            builder.Property(e => e.FormerRegisteredCompanyName)
             .HasMaxLength(150);

            builder.Property(e => e.ForOthers)
            .HasMaxLength(150);

            builder.Property(e => e.ContactPersonName)
            .HasMaxLength(150);

            //Phase 3 Changes
            builder.Property(a => a.FullName)
            .HasMaxLength(150);

            builder.Property(e => e.PersonInChargeName)
            .HasMaxLength(150);

            builder.Property(e => e.AliasName)
            .HasMaxLength(150);




            builder.HasOne(o => o.NationalityMeta)
                .WithMany()
                .HasForeignKey("NationalityMetaCode");


            //Phase 3 Sprint 2 Changes

            builder.HasOne(a => a.BusinessProfileIDType)
                    .WithMany()
                    .HasForeignKey("BusinessProfileIDTypeCode")
                    .IsRequired(false);

            builder.Property(a => a.IDNumber)
                .HasMaxLength(150);

            builder.HasOne(o => o.ServiceType)
            .WithMany()
           .HasForeignKey("ServiceTypeCode");

            builder.HasOne(o => o.CollectionTier)
            .WithMany()
            .HasForeignKey("CollectionTierCode");


            builder.HasOne(o => o.BusinessKYCStatus)
                    .WithMany()
                    .HasForeignKey("BusinessKYCStatusCode");

            builder.HasOne(o => o.BusinessWorkflowStatus)
                .WithMany()
                .HasForeignKey("BusinessWorkflowStatusCode")
                .IsRequired(false);

            builder.HasOne(o => o.BusinessKYCSubmissionStatus)
               .WithMany()
               .HasForeignKey("BusinessKYCSubmissionStatusCode")
               .IsRequired(false);

            builder.Property(o => o.ReviewConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.ReviewAndFeedbackConcurrencyToken).IsConcurrencyToken();

            //Ownership Concurrency Token
            builder.Property(o => o.ShareholderConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.CompanyShareholderConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.IndividualShareholderConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.LegalEntityConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.CompanyLegalEntityConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.IndividualLegalEntityConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.ParentHoldingsConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.BoardOfDirectorConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.PrimaryOfficerConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.PoliticalExposedPersonsConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.AffiliatesAndSubsidiariesConcurrencyToken).IsConcurrencyToken();
            builder.Property(o => o.AuthorisedPersonConcurrencyToken).IsConcurrencyToken();
            //AMLCFT Concurrency Token
            builder.Property(o => o.AMLCFTQuestionnaireConcurrencyToken).IsConcurrencyToken();

            //ticket 55839
            builder.Property(e => e.SSTRegistrationNumber)
                    .HasMaxLength(17);

            builder.HasOne(o => o.Title)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("TitleCode");

            builder.Property(o => o.TitleOthers)
                .HasMaxLength(50);

        }
    }


}
