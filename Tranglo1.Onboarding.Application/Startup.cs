using AutoMapper;
using CorrelationId;
using CorrelationId.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tranglo1.Onboarding.Application.DependencyInjection;
using Tranglo1.Onboarding.Application.Hubs;
using Tranglo1.Onboarding.Application.Infrastructure.Persistance;
using Tranglo1.Onboarding.Application.Infrastructure.Swagger;
using Tranglo1.Onboarding.Application.Security;
using Tranglo1.Onboarding.Application.Validations;
using Tranglo1.Onboarding.Infrastructure.DependencyInjection;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            // Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration.GetValue<string>("IdentityServerUri");
                    options.RequireHttpsMetadata = !Environment.IsDevelopment();
                    options.Audience = "api1";
                });

            // Authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthenticationPolicies.InternalOnlyPolicy,
                    AuthenticationPolicies.CreateInternalUserOnlyPolicy());
                options.AddPolicy(AuthenticationPolicies.ExternalOnlyPolicy,
                    AuthenticationPolicies.CreateExternalUserOnlyPolicy());
                options.AddPolicy(AuthenticationPolicies.InternalOrExternalPolicy,
                    AuthenticationPolicies.CreateInternalOrExternalUserPolicy());
                options.AddPolicy(AuthenticationPolicies.ExternalConnectOnlyPolicy,
                    AuthenticationPolicies.CreateExternalConnectUserOnlyPolicy());
                options.AddPolicy(AuthenticationPolicies.ExternalBusinessOnlyPolicy,
                    AuthenticationPolicies.CreateExternalBusinessUserOnlyPolicy());
            });

            // Controllers + Newtonsoft JSON
            services.AddControllers()
                .AddNewtonsoftJson();

            // API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Tranglo Onboarding API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityServerUri")}/connect/authorize"),
                            TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityServerUri")}/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID" },
                                { "profile", "Profile" },
                                { "api1.read", "API Read" }
                            }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
                options.OperationFilter<FileOperationFilter>();
                options.OperationFilter<SwaggerDefaultValues>();
                options.EnableAnnotations();
            });

            // MediatR
            services.AddMediatR(OnboardingServiceCollectionExtensions.OnboardingApplicationAssembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLogBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UserAccessControlBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));

            // AutoMapper
            services.AddAutoMapper(OnboardingServiceCollectionExtensions.OnboardingApplicationAssembly);

            // FluentValidation
            services.AddValidatorsFromAssembly(OnboardingServiceCollectionExtensions.OnboardingApplicationAssembly);

            // DbContexts
            services.AddDbContext<BusinessProfileDbContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<ApplicationUserDbContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<PartnerDBContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<ScreeningDBContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<RBADBContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<RBARequisitionDBContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<KYCPartnerStatusRequisitionDbContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<ExternalUserRoleDbContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<SignUpCodeDBContext>(opts =>
                opts.UseSqlServer(connectionString));
            services.AddDbContext<AuditLogDbContext>(opts =>
                opts.UseSqlServer(Configuration.GetConnectionString("AuditLogConnection") ?? connectionString));

            // Event Dispatcher
            services.AddNullEventDispatcher();

            // Infrastructure & Application services
            services.AddOnboardingInfrastructure(Configuration, Environment);
            services.AddOnboardingApplicationServices();
            services.AddScoped<IAuditLogManager, AuditLogManager>();
            services.AddSqlServerUnitOfWork(connectionString);
            services.AddWebIdentityContext();

            // User Access Control
            services.AddSingleton<AccessControlManager>();

            // SignalR
            services.AddSignalR();

            // Health Checks
            services.AddHealthChecks()
                .AddSqlServer(connectionString, name: "sql-server");

            // Correlation ID
            services.AddCorrelationId();

            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowedOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
                    if (allowedOrigins != null && allowedOrigins.Length > 0)
                        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    else
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCorrelationId();
            app.UseSerilogRequestLogging();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tranglo Onboarding API v1");
                options.OAuthClientId(Configuration.GetValue<string>("SwaggerClientId"));
                options.OAuthUsePkce();
            });

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseErrorHandlingMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<UserLogOffHub>("/hubs/userlogoff");
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
