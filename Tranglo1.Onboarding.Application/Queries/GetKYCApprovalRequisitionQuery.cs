using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.Onboarding.Domain.Entities.Requisition;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCPartnerKYCApprovalList, UACAction.View)]
    internal class GetKYCApprovalRequisitionQuery : BaseQuery<Result<PartnerKYCStatusRequisition>>
    {
        public string RequisitionCode { get; set; }
        internal class GetKYCApprovalRequisitionQueryHandler : IRequestHandler<GetKYCApprovalRequisitionQuery, Result<PartnerKYCStatusRequisition>>
        {
            private readonly ApprovalManager<PartnerKYCStatusRequisition> _approvalManager;

            public GetKYCApprovalRequisitionQueryHandler(ApprovalManager<PartnerKYCStatusRequisition> approvalManager)
            {
                _approvalManager = approvalManager;
            }

            public async Task<Result<PartnerKYCStatusRequisition>> Handle(GetKYCApprovalRequisitionQuery request, CancellationToken cancellationToken)
            {
                var requisition = await _approvalManager.GetRequisitionByCodeAsync(request.RequisitionCode);

                if(requisition == null)
                {
                    return Result.Failure<PartnerKYCStatusRequisition>("Requisiont Not Found");
                }

                return Result.Success(requisition);
            }
        }
    }
}
