using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.Requisition;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RequisitionEntityTypeConfiguration
{
    class PartnerKYCStatusRequisitionEntityTypeConfiguration : BaseEntityTypeConfiguration<PartnerKYCStatusRequisition>
    {
        protected override void Configure(EntityTypeBuilder<PartnerKYCStatusRequisition> builder)
        {
            builder.ToTable("PartnerKYCStatusRequisitions", KYCPartnerStatusRequisitionDbContext.DEFAULT_SCHEMA);

            builder.Property(kyc => kyc.Id)
                   .HasColumnName("PartnerKYCStatusRequisitionId");
            builder.HasKey(kyc => kyc.Id);
        }

        protected override bool HasCreationInfo()
        {
            return false;
        }

        protected override bool HasLastModificationInfo()
        {
            return false;
        }
    }
}
