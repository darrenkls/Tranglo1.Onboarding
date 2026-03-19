using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloStaff : ApplicationUser
    {
        protected TrangloStaff() { }

        public TrangloStaff(string loginId, FullName fullName, Email email)
        {
            if (string.IsNullOrEmpty(loginId))
                throw new ArgumentException(nameof(loginId));

            LoginId = loginId;
            SetName(fullName);
            SetEmail(email);
            SetAccountStatus(AccountStatus.Active);
        }

        private readonly List<TrangloStaffEntityAssignment> _trangloStaffEntityAssignments = new List<TrangloStaffEntityAssignment>();
        public IReadOnlyList<TrangloStaffEntityAssignment> TrangloStaffEntityAssignments => _trangloStaffEntityAssignments.AsReadOnly();
    }
}
