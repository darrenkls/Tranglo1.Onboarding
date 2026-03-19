using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
	class KYCSummaryFeedbackEntityTypeConfiguration : BaseEntityTypeConfiguration<KYCSummaryFeedback>
	{
		protected override void Configure(EntityTypeBuilder<KYCSummaryFeedback> builder)
		{
			builder.ToTable("KYCSummaryFeedback", BusinessProfileDbContext.DEFAULT_SCHEMA);


			//Primary Key
			builder.Property(kyc => kyc.Id)
					.HasColumnName("KYCSummaryFeedbackCode");
			builder.HasKey(kyc => kyc.Id);

			builder.HasOne(o => o.KYCCategory)
			  .WithMany()
			  .IsRequired()
			  .HasForeignKey("KYCCategoryCode");

			builder.HasOne(o => o.BusinessProfile)
			  .WithMany()
			  .IsRequired()
			  .HasForeignKey("BusinessProfileCode");

			builder.Property(o => o.IncorrectItem)
				.HasMaxLength(150);

			builder.Property(o => o.InternalRemarks)
				.HasMaxLength(500);

			builder.Property(o => o.FeedbackToUser)
				.HasMaxLength(500);

			builder.Property(o => o.IsResolved)
				.HasDefaultValue(0);

		}
	}
}
