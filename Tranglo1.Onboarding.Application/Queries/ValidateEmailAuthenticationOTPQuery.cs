using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Queries
{
    public class ValidateEmailAuthenticationOTPQuery : IRequest<Result<long>>
    {
        public string MFAOTP { get; set; }

        public class ValidateEmailAuthenticationOTPQueryHandler : IRequestHandler<ValidateEmailAuthenticationOTPQuery, Result<long>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<ValidateEmailAuthenticationOTPQueryHandler> _logger;

            public ValidateEmailAuthenticationOTPQueryHandler(IConfiguration config, ILogger<ValidateEmailAuthenticationOTPQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<long>> Handle(ValidateEmailAuthenticationOTPQuery request, CancellationToken cancellationToken)
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");

                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        var parameters = new DynamicParameters();
                        parameters.Add("@MFAOTP", request.MFAOTP);
                        parameters.Add("@Result", dbType: DbType.Int64, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync(
                            "ValidateEmailAuthenticationOTP",
                            parameters,
                            commandType: CommandType.StoredProcedure);

                        var result = parameters.Get<long>("@Result");

                        if (result <= 0)
                        {
                            return Result.Failure<long>("Invalid or expired OTP.");
                        }

                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{0}]", nameof(ValidateEmailAuthenticationOTPQueryHandler));
                    return Result.Failure<long>("Unable to validate email authentication OTP.");
                }
            }
        }
    }
}
