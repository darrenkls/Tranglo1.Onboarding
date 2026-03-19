using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloStaff : ApplicationUser
    {
        protected TrangloStaff() : base() { }

        public TrangloStaff(string loginId, FullName fullName, Email email)
            : base(fullName, email)
        {
            if (string.IsNullOrEmpty(loginId))
                throw new ArgumentException(nameof(loginId));

            LoginId = loginId;
            SetAccountStatus(AccountStatus.Active);
        }

        private List<TrangloStaffAssignment> _trangloStaffAssignments = new List<TrangloStaffAssignment>();
        public IReadOnlyList<TrangloStaffAssignment> TrangloStaffAssignments => _trangloStaffAssignments.AsReadOnly();

        private List<TrangloStaffEntityAssignment> _trangloStaffEntityAssignments = new List<TrangloStaffEntityAssignment>();
        public IReadOnlyList<TrangloStaffEntityAssignment> TrangloStaffEntityAssignments => _trangloStaffEntityAssignments.AsReadOnly();

        public TrangloStaffAssignment AssignToTrangloEntityAssignment(TrangloEntity trangloEntity, TrangloDepartment trangloDepartment, UserRole userRole)
        {
            var existing = _trangloStaffAssignments.Find(x =>
                x.LoginId == LoginId &&
                x.TrangloEntity == trangloEntity.TrangloEntityCode &&
                x.TrangloDepartmentCode == trangloDepartment.Id &&
                x.RoleCode == userRole.Id.ToString());

            if (existing != null)
                return existing;

            var assignment = new TrangloStaffAssignment(trangloDepartment.Id, userRole.Id.ToString(), trangloEntity.TrangloEntityCode, LoginId)
            {
                LoginId = LoginId,
                TrangloEntity = trangloEntity.TrangloEntityCode,
                TrangloDepartmentCode = trangloDepartment.Id,
                RoleCode = userRole.Id.ToString()
            };

            _trangloStaffAssignments.Add(assignment);
            AssignToTrangloStaffEntityAssignment(trangloEntity);

            return assignment;
        }

        public TrangloStaffEntityAssignment AssignToTrangloStaffEntityAssignment(TrangloEntity trangloEntity)
        {
            var existing = _trangloStaffEntityAssignments.Find(x =>
                x.LoginId == LoginId &&
                x.TrangloEntity == trangloEntity.TrangloEntityCode);

            if (existing != null)
                return existing;

            var entityAssignment = new TrangloStaffEntityAssignment(trangloEntity.TrangloEntityCode, LoginId)
            {
                LoginId = LoginId,
                TrangloEntity = trangloEntity.TrangloEntityCode,
                AccountStatus = Enumeration.FindById<CompanyUserAccountStatus>(CompanyUserAccountStatus.Active.Id),
                BlockStatus = Enumeration.FindById<CompanyUserBlockStatus>(CompanyUserBlockStatus.Unblock.Id)
            };

            _trangloStaffEntityAssignments.Add(entityAssignment);
            return entityAssignment;
        }

        public void RemoveTrangloEntityAssignment(TrangloEntity trangloEntity, TrangloDepartment trangloDepartment, UserRole userRole)
        {
            var assignment = _trangloStaffAssignments.Find(x =>
                x.LoginId == LoginId &&
                x.TrangloEntity == trangloEntity.TrangloEntityCode &&
                x.TrangloDepartmentCode == trangloDepartment.Id &&
                x.RoleCode == userRole.Id.ToString());

            if (assignment != null)
                _trangloStaffAssignments.Remove(assignment);

            var stillHasEntity = _trangloStaffAssignments.Find(x =>
                x.TrangloEntity == trangloEntity.TrangloEntityCode && x.LoginId == LoginId);

            if (stillHasEntity == null)
            {
                var entityAssignment = _trangloStaffEntityAssignments.Find(x =>
                    x.LoginId == LoginId && x.TrangloEntity == trangloEntity.TrangloEntityCode);

                if (entityAssignment != null)
                    _trangloStaffEntityAssignments.Remove(entityAssignment);
            }
        }
    }
}
