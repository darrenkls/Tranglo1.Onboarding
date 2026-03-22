using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.OTP;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly ApplicationUserDbContext _dbContext;

        private class RequisitionOTPWithDate : RequisitionOTP
        {
            public DateTime CreatedDate { get; set; }
        }

        public OtpRepository(ApplicationUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task NewRequisitionOTPAsync(RequisitionOTP requisitionOTP)
        {
            _dbContext.RequisitionOTPs.Add(requisitionOTP);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ValidateOTPAsync(RequisitionOTP requisitionOTP, int userId)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString))
            {
                await connection.OpenAsync();
                var result = (await connection.QueryAsync<RequisitionOTPWithDate>(
                    "GetLatestUserOTP",
                    new
                    {
                        @RequisitionCode = requisitionOTP.RequisitionCode,
                        @CreatedBy = userId
                    },
                    null, null, CommandType.StoredProcedure)).ToList();

                if (result.Count == 0)
                    return false;

                var otpResult = result.First();
                return otpResult.RequestID == requisitionOTP.RequestID
                    && otpResult.OTP == requisitionOTP.OTP
                    && DateTime.UtcNow < otpResult.CreatedDate.AddMinutes(5);
            }
        }
    }
}
