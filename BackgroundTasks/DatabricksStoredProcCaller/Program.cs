using Masking.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tranglo1.DatabricksStoredProcCaller
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            var BUILD_VERSION = Environment.GetEnvironmentVariable("BUILD_VERSION");

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);

            if (environment.StartsWith("Cloud-", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Running in container");

                configBuilder.AddSystemsManager(aws =>
                {
                    Console.WriteLine($"environment = {environment}");

                    aws.Path = $"/{environment}/{nameof(DatabricksStoredProcCaller)}";
                    aws.ReloadAfter = TimeSpan.FromSeconds(30);
                    aws.Optional = true;
                });
            }

            var config = configBuilder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Destructure.ByMaskingProperties(x =>
                {
                    x.PropertyNames.Add("Password");
                    x.PropertyNames.Add("Token");
                    x.PropertyNames.Add("Cookie");
                    x.PropertyNames.Add("ApiKey");
                    x.Mask = "******";
                })
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level} {CorrelationId}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            Log.Information($"DOTNET_ENVIRONMENT : {environment}");
            Log.Information($"BUILD_VERSION : {BUILD_VERSION}");

            LogContext.PushProperty("CorrelationId", Guid.NewGuid().ToString());

            try
            {
                string workspaceUrl = config["Databricks:WorkspaceUrl"];
                string token        = config["Databricks:Token"];
                string warehouseId  = config["Databricks:WarehouseId"];
                string salesDate    = config["Databricks:SalesDate"];

                if (string.IsNullOrWhiteSpace(workspaceUrl) ||
                    string.IsNullOrWhiteSpace(token) ||
                    string.IsNullOrWhiteSpace(warehouseId))
                {
                    Log.Error("One or more required Databricks configuration values are missing (WorkspaceUrl, Token, WarehouseId).");
                    return;
                }

                string sql = $"CALL update_sales_by_date('{salesDate}')";

                Log.Information($"Executing SQL: {sql}");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var payload = new
                {
                    statement    = sql,
                    warehouse_id = warehouseId
                };

                var json = JsonSerializer.Serialize(payload);

                Log.Information("Sending request to Databricks SQL Statements API.");

                var response = await client.PostAsync(
                    $"{workspaceUrl}/api/2.0/sql/statements",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Log.Information($"Databricks API call succeeded. Response: {result}");
                }
                else
                {
                    Log.Error($"Databricks API call failed. StatusCode: {(int)response.StatusCode} {response.StatusCode}. Response: {result}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
