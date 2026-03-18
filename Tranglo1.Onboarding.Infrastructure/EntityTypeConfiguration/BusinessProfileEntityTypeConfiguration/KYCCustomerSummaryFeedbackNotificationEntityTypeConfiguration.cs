using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class KYCCustomerSummaryFeedbackNotificationEntityTypeConfiguration : BaseEntityTypeConfiguration<KYCCustomerSummaryFeedbackNotification>
    {
        protected override void Configure(EntityTypeBuilder<KYCCustomerSummaryFeedbackNotification> builder)
        {
            builder.ToTable("KYCCustomerSummaryFeedbackNotification", BusinessProfileDbContext.DEFAULT_SCHEMA);

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("KYCCustomerSummaryFeedbackNotificationCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.KYCCustomerSummaryFeedback)
              .WithMany()
              .IsRequired()
              .HasForeignKey("KYCCustomerSummaryFeedbackCode");

            builder.HasOne(o => o.BusinessProfile)
              .WithMany()
              .IsRequired()
              .HasForeignKey("BusinessProfileCode");

            builder.Property(o => o.Event)
                .HasMaxLength(20);
        }
    }
}
