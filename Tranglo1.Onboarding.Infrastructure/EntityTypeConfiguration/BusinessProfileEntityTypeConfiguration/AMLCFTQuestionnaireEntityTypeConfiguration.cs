using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class AMLCFTQuestionnaireEntityTypeConfiguration : BaseEntityTypeConfiguration<AMLCFTQuestionnaire>
    {
        protected override void Configure(EntityTypeBuilder<AMLCFTQuestionnaire> builder)
        {
            builder.ToTable("AMLCFTQuestionnaires", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("AMLCFTQuestionnaires");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("AMLCFTQuestionnaireCode");
            builder.HasKey(kyc => kyc.Id);

            builder.HasOne(o => o.Question)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("QuestionCode");

            builder.HasOne(o => o.BusinessProfile)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("BusinessProfileCode");
        }
    }
}
