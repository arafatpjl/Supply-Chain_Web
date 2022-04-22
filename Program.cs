using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using SupplyChain.Application;
using SupplyChain.Application.Abstractions;
using SupplyChain.Infrastructure;
using SupplyChain.Infrastructure.Persistence;
using SupplyChain.Infrastructure.Persistence.Seed;
using SupplyChain.Application.Authorization;
using SupplyChain.Web.Services;

// Bootstrap logger, so failures during start-up are still recorded. The legacy application had
// no logging of any kind: a start-up failure produced a MsgBox and nothing else.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "SupplyChain")
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine("logs", "supplychain-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    // ---- Composition root -------------------------------------------------------------------
    // Each layer registers its own services, so this file stays a wiring summary rather than an
    // inventory of every type in the system.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddScoped<ITenantContext, TenantContext>();
    // Permissions are claims on the user and on their roles. Identity's own claims factory
    // emits both into the principal at sign-in, so no per-request transformation is needed.
    //
    // Claims are baked into the cookie, which would leave a user holding permissions that had
    // just been revoked. Every change that affects access updates the user's security stamp, and
    // a short validation interval means the principal is rebuilt within a minute of that.
    builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    {
        options.ValidationInterval = TimeSpan.FromMinutes(1);
    });

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

    // One policy per permission. Registering them from the constants list means a new permission
    // cannot be referenced by a controller without also being enforceable.
    builder.Services.AddAuthorization(options =>
    {
        foreach (var permission in Permissions.All)
        {
            options.AddPolicy(permission, policy =>
                policy.RequireClaim(Permissions.ClaimType, permission));
        }

        // Authentication is required by default; anonymous access must be opted into explicitly
        // with [AllowAnonymous]. The legacy application's equivalent was hiding menu items, which
        // protected nothing.
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    // QuestPDF requires the licence to be declared before any document is generated. The
    // Community licence covers organisations under the published revenue threshold; a deployment
    // at Pacific Jeans Ltd. would likely need a paid licence. See docs/05-Reporting-Strategy.md §5.
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

        // MVC infers a [Required] rule for every non-nullable reference type and reports it as
        // "The X field is required." That default silently replaced the legacy wording this
        // migration deliberately preserves — a master-data form answered "The Name field is
        // required." where the VB6 form said "Type name of Country". Validation messages are
        // owned explicitly here, by data annotations or by the Application layer, so the
        // inferred rule is suppressed.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<SupplyChainDbContext>("database", tags: ["ready"]);

    var app = builder.Build();

    // ---- Pipeline ---------------------------------------------------------------------------
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
    }).AllowAnonymous();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
    }).AllowAnonymous();

    await SeedDatabaseAsync(app);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "The application terminated unexpectedly during start-up");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Applies migrations and seeds baseline data, when configuration asks for it.
static async Task SeedDatabaseAsync(WebApplication app)
{
    if (!app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    // Read from configuration so no credential is ever compiled into the assembly. The legacy
    // source carried a live SQL Server password in modMain.bas in plain text.
    var adminPassword = app.Configuration["Database:AdminPassword"]
        ?? throw new InvalidOperationException(
            "Database:SeedOnStartup is enabled but Database:AdminPassword is not set. " +
            "Provide it via user secrets or the Database__AdminPassword environment variable.");

    await seeder.SeedAsync(adminPassword);
}

/// <summary>Exposed so the integration test project can reference the entry-point assembly.</summary>
public partial class Program;
