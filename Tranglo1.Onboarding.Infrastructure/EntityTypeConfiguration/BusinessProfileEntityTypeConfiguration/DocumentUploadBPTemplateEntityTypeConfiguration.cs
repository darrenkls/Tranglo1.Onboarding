using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class DocumentUploadBPEntityTypeConfiguration : BaseEntityTypeConfiguration<DocumentUploadBP>
    {
        protected override void Configure(EntityTypeBuilder<DocumentUploadBP> builder)
        {
            builder.ToTable("DocumentUploadBPs", BusinessProfileDbContext.DEFAULT_SCHEMA);
            builder.HasTemporalTable(config =>
            {
                config.HistorySchema(BusinessProfileDbContext.HISTORY_SCHEMA);
                config.HistoryTable("DocumentUploadBPs");
            });


            builder.HasOne(o => o.DocumentCategoryBP)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey("DocumentCategoryBPCode")
                .OnDelete(DeleteBehavior.Cascade);

            //Primary Key
            builder.HasKey(kyc => new { kyc.DocumentCategoryBPCode, kyc.DocumentId });

        }
    }
}
