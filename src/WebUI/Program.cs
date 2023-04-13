using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using WebUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebUIServices();

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.MinimumLevel.Debug()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
      .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
      .MinimumLevel.Override("System", LogEventLevel.Warning)
      .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        theme: AnsiConsoleTheme.Code)
      .Enrich.FromLogContext()
      .ReadFrom.Configuration(builder.Configuration);
});

Log.Information("Starting up");

var CorsOrigins = "_CorsOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsOrigins,
                      policy =>
                      {
                          policy.
                          SetIsOriginAllowedToAllowWildcardSubdomains()
                          .WithOrigins("https://localhost:5173",
                                       "http://localhost:5173",
                                      "localhost:5173")
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .Build();
                      });
});

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // TODO: Do this properly. https://stackoverflow.com/questions/69961449/net6-and-datetime-problem-cannot-write-datetime-with-kind-utc-to-postgresql-ty

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();

    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi3(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.UseCors(CorsOrigins);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.UseEndpoints(endpoints =>
{
    var pipeline = endpoints.CreateApplicationBuilder().Build();
    var oidcAuthAttr = new AuthorizeAttribute { AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme };
    endpoints
        .Map("/api/{documentName}/specification.json", pipeline)
        .RequireAuthorization(oidcAuthAttr);
    endpoints
        .Map("/api/index.html", pipeline)
        .RequireAuthorization(oidcAuthAttr);
});


app.Run();