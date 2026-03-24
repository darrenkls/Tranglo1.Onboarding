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

namespace Tranglo1.Onboarding.Application.Command
{
    public class RequestEmailAuthenticationOTPCommand : IRequest<Result<long>>
    {
        public string LoginId { get; set; }

        public class RequestEmailAuthenticationOTPCommandHandler : IRequestHandler<RequestEmailAuthenticationOTPCommand, Result<long>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<RequestEmailAuthenticationOTPCommandHandler> _logger;

            public RequestEmailAuthenticationOTPCommandHandler(IConfiguration config, ILogger<RequestEmailAuthenticationOTPCommandHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<long>> Handle(RequestEmailAuthenticationOTPCommand request, CancellationToken cancellationToken)
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");

                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        var parameters = new DynamicParameters();
                        parameters.Add("@LoginId", request.LoginId);
                        parameters.Add("@Result", dbType: DbType.Int64, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync(
                            "RequestEmailAuthenticationOTP",
                            parameters,
                            commandType: CommandType.StoredProcedure);

                        var result = parameters.Get<long>("@Result");

                        if (result <= 0)
                        {
                            return Result.Failure<long>("Unable to request email authentication OTP.");
                        }

                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{0}]", nameof(RequestEmailAuthenticationOTPCommandHandler));
                    return Result.Failure<long>("Unable to request email authentication OTP.");
                }
            }
        }
    }
}
