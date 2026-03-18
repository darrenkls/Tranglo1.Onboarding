using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.ApprovalWorkflowEngine.Models;
using Tranglo1.Onboarding.Domain.Entities.Requisition;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.EntityTypeConfiguration.RequisitionEntityTypeConfiguration
{
    class PartnerKYCStatusRequisitionHistoryEntityTypeConfiguration : BaseEntityTypeConfiguration<ApprovalHistory<PartnerKYCStatusRequisition>>
    {
        protected override void Configure(EntityTypeBuilder<ApprovalHistory<PartnerKYCStatusRequisition>> builder)
        {
            builder.ToTable("PartnerKYCStatusRequisitionHistories", KYCPartnerStatusRequisitionDbContext.DEFAULT_SCHEMA);

            builder.Property(kyc => kyc.Id)
                   .HasColumnName("PartnerKYCStatusRequisitionHistoryCode");
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
