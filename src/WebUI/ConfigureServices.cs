using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using WebUI.Services;
using ZymLabs.NSwag.FluentValidation;

namespace WebUI;

public static class ConfigureServices
{
    public static IServiceCollection AddWebUIServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddHttpContextAccessor();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddControllersWithViews();

        services.AddRazorPages();

        services.AddScoped<FluentValidationSchemaProcessor>(provider =>
        {
            var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
            var loggerFactory = provider.GetService<ILoggerFactory>();

            return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
        });

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.AddOpenApiDocument((configure, serviceProvider) =>
        {
            var fluentValidationSchemaProcessor = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<FluentValidationSchemaProcessor>();

            // Add the fluent validations schema processor
            configure.SchemaProcessors.Add(fluentValidationSchemaProcessor);

            configure.Title = "CleanArchitecture API";
            configure.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.OAuth2,
                Description = "My Authentication",
                Flow = OpenApiOAuth2Flow.Implicit,
                Flows = new OpenApiOAuthFlows()
                {
                    Implicit = new OpenApiOAuthFlow()
                    {
                        Scopes = new Dictionary<string, string>
                {
                    { "read", "Read access to protected resources" },
                    { "write", "Write access to protected resources" }
                },
                        AuthorizationUrl = "https://localhost:44333/core/connect/authorize",
                        TokenUrl = "https://localhost:44333/core/connect/token"
                    },
                }
            });

            configure.OperationProcessors.Add(
                new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            //      new OperationSecurityScopeProcessor("bearer"));

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        return services;
    }
}
