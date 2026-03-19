using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class AnswerChoiceEntityTypeConfiguration : BaseEntityTypeConfiguration<AnswerChoice>
    {
        protected override void Configure(EntityTypeBuilder<AnswerChoice> builder)
        {
            builder.ToTable("AnswerChoices", BusinessProfileDbContext.DEFAULT_SCHEMA);

            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("AnswerChoiceCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(kyc => kyc.Description)
                    .HasColumnName("AnswerChoiceDescription");

            builder.HasOne(o => o.Question)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("QuestionCode");

        }
    }
}
