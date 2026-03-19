using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
	class KYCCustomerSummaryFeedbackEntityTypeConfiguration : BaseEntityTypeConfiguration<KYCCustomerSummaryFeedback>
	{
		protected override void Configure(EntityTypeBuilder<KYCCustomerSummaryFeedback> builder)
		{
			builder.ToTable("KYCCustomerSummaryFeedback", BusinessProfileDbContext.DEFAULT_SCHEMA);
			builder.HasTemporalTable(configuration =>
			{
				configuration.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
				configuration.HistoryTable("KYCCustomerSummaryFeedbacks");
			});



			//Primary Key
			builder.Property(kyc => kyc.Id)
					.HasColumnName("KYCCustomerSummaryFeedbackCode");
			builder.HasKey(kyc => kyc.Id);

			builder.HasOne(o => o.KYCCategory)
			  .WithMany()
			  .IsRequired()
			  .HasForeignKey("KYCCategoryCode");

			builder.HasOne(o => o.BusinessProfile)
			  .WithMany()
			  .IsRequired()
			  .HasForeignKey("BusinessProfileCode");

			builder.Property(o => o.FeedbackToTranglo)
				.HasMaxLength(500);


		}
	}
}
