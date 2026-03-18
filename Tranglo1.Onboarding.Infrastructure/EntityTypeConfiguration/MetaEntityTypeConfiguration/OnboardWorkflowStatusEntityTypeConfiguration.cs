using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration
{
    class OnboardWorkflowStatusEntityTypeConfiguration : BaseEntityTypeConfiguration<OnboardWorkflowStatus>
    {     

        protected override void Configure(EntityTypeBuilder<OnboardWorkflowStatus> builder)
        {
            builder.ToTable("OnboardWorkflowStatuses", PartnerDBContext.META_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .IsRequired()
                .HasColumnName("OnboardWorkflowStatusCode");

            builder.Property(o => o.Name)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("Description");

            builder.HasData(Enumeration.GetAll<OnboardWorkflowStatus>());
        }
    }
}
