using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.MetaEntityTypeConfiguration
{
    class TikCustomerCategoriesEntityTypeConfiguration : BaseEntityTypeConfiguration<TikCustomerCategories>
    {
        protected override void Configure(EntityTypeBuilder<TikCustomerCategories> builder)
        {
            builder.ToTable("TikCustomerCategories", ApplicationUserDbContext.META_SCHEMA);

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .IsRequired()
                   .HasColumnName("CustomerCategoryCode");

            builder.Property(a => a.Name)
                .IsRequired()
                .HasColumnName("Description");

            builder.Property(a => a.CustomerTypeCode)
                 .IsRequired()
                 .HasColumnName("CustomerTypeCode");

            builder.HasData(
                TikCustomerCategories.Crypto_Currency_Exchange, 
                TikCustomerCategories.Mass_Payout, 
                TikCustomerCategories.Normal_Corporate, 
                TikCustomerCategories.Remittance_Partner
                );
        }
    }
}
