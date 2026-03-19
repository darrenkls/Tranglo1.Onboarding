using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class AMLCFTQuestionnaireAnswerEntityTypeConfiguration : BaseEntityTypeConfiguration<AMLCFTQuestionnaireAnswer>
    {
        protected override void Configure(EntityTypeBuilder<AMLCFTQuestionnaireAnswer> builder)
        {
            builder.ToTable("AMLCFTQuestionnaireAnswers", BusinessProfileDbContext.DEFAULT_SCHEMA);

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("AMLCFTQuestionnaireAnswerCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(kyc => kyc.AnswerRemark)
                    .HasMaxLength(150);

            builder.HasOne(o => o.AnswerChoice)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("AnswerChoiceCode");

            builder.HasOne(o => o.AMLCFTQuestionnaire)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("AMLCFTQuestionnaireCode");
        }
    }
}
