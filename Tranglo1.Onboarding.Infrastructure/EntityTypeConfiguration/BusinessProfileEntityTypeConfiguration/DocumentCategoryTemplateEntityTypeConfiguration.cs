using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentCategoryTemplateEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentCategoryTemplate>
    {
        protected override void Configure(EntityTypeBuilder<DocumentCategoryTemplate> builder)
        {
            builder.ToTable("DocumentCategoryTemplates", BusinessProfileDbContext.DEFAULT_SCHEMA);

            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("DocumentCategoryTemplates");
            });


            builder.HasOne(o => o.Questionnaire)
              .WithMany()
              .IsRequired(false)
              .HasForeignKey("QuestionnaireCode")
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.DocumentCategory)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCategoryCode")
                .OnDelete(DeleteBehavior.Cascade);

            //Primary Key
            //builder.HasKey(kyc => new { kyc.DocumentCategoryCode, kyc.DocumentId });
            builder.Property(o => o.Id)
           .IsRequired()
           .HasColumnName("DocumentCategoryTemplateCode");
            builder.HasKey(x => x.Id);
            builder.HasIndex(p => new { p.DocumentCategoryCode, p.QuestionnaireCode }).IsUnique();

        }
    }
}
