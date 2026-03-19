using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentCategoryEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentCategory>
    {
        protected override void Configure(EntityTypeBuilder<DocumentCategory> builder)
        {
            builder.ToTable("DocumentCategories", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("DocumentCategories");
            });


            //Primary Key
            builder.Property(kyc => kyc.Id)
                    .HasColumnName("DocumentCategoryCode");
            builder.HasKey(kyc => kyc.Id);

            builder.Property(e => e.DocumentCategoryDescription)
                    .HasMaxLength(500);

            builder.HasOne(o => o.DocumentCategoryGroup)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCategoryGroupCode");

            

        }
    }
}
