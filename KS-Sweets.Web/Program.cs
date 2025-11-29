using KS_Sweets.Application.CrossCuttingConcerns.Logging;
using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities.Identity;
using KS_Sweets.Infrastructure.Data.Context;
using KS_Sweets.Infrastructure.DI;
using KS_Sweets.Infrastructure.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// =========================================================
// 1. WEB APPLICATION BUILDER
// =========================================================
var builder = WebApplication.CreateBuilder(args);

// Configure services for dependency injection
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// =========================================================
// 2. MIDDLEWARE CONFIGURATION
// =========================================================
ConfigureMiddleware(app);

// Seed database roles (must run after app.Build() but before app.Run())
SeedDatabaseRoles(app);

app.Run();

// =========================================================
// 3. SERVICE CONFIGURATION (ADD SERVICES)
// =========================================================
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // --- Infrastructure Services (DB, Identity, DI Container) ---

    // Configure Database Context
    ConfigureDatabase(services, configuration);

    // Configure Identity
    ConfigureIdentity(services);

    // Configure Identity Cookie Settings
    ConfigureApplicationCookie(services);

    // Register the external authentication handler (Google)
    ExternalAuthentication(services, configuration);

    // Configure Custom Email Settings (Configuration mapping)
    ConfigureEmailSettings(services, configuration);

    // Register all Application / Infrastructure Dependencies from DI layer
    RegisterApplicationServices(services);

    // --- Framework Services (MVC, RazorPages, Session, Caching) ---

    // Add services to the container (MVC and API Configuration).
    services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            // Fixes potential serialization issues with circular references (e.g., entity relationships)
            options.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    services.AddRazorPages();

    // Required for accessing HttpContext outside of Controllers/Pages (e.g., custom logging, services)
    services.AddHttpContextAccessor();

    // Used primarily in development to catch unhandled database exceptions
    services.AddDatabaseDeveloperPageExceptionFilter();

    // Configure caching for Session/TempData
    services.AddDistributedMemoryCache();

    // Configure Session
    ConfigureSession(services);
}

// ====================== Database Connection ======================
void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("KSSweetsDbConnection");

    // Check for missing connection string in a robust way
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'KSSweetsDbConnection' not found in configuration.");
    }
    // Add DbContext with SQL Server
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// ====================== Identity Configuration ======================
void ConfigureIdentity(IServiceCollection services)
{
    // Uses the custom ApplicationUser and IdentityRole
    services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        // Recommended for security in production, but often set to false during development
        options.SignIn.RequireConfirmedAccount = false;

        // Optional: Add strong password requirements here
        // options.Password.RequireLowercase = true;
    })
   // Ensure AddRoles is used to support RoleManager and authorization
   .AddRoles<IdentityRole>()
   .AddEntityFrameworkStores<ApplicationDbContext>();
}
void ConfigureApplicationCookie(IServiceCollection services)
{
    services.ConfigureApplicationCookie(options =>
    {
        // Define paths for redirection
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";

        // Sets the maximum age for the authentication cookie
        options.ExpireTimeSpan = TimeSpan.FromDays(7);

        // Allows the cookie lifetime to be reset upon each request (good user experience)
        options.SlidingExpiration = true;
    });
}

void ExternalAuthentication(IServiceCollection services, IConfiguration configuration)
{
    // ====================== Google Authentication ======================
    services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = configuration.GetSection("GoogleKeys:ClientId").Value;
            googleOptions.ClientSecret = configuration.GetSection("GoogleKeys:ClientSecret").Value;

            // Google Cloud Console
            googleOptions.CallbackPath = "/signin-google";
        });
}
void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
{
    // Maps the "EmailSettings" configuration section to the EmailSettings POCO class
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
}
void RegisterApplicationServices(IServiceCollection services)
{
    // Calls the extension method to register services defined in the Infrastructure.DI layer
    services.AddInfrastructureDependencies();
}
void ConfigureSession(IServiceCollection services)
{
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(100); // 100 minutes of inactivity before session expires
        options.Cookie.HttpOnly = true; // Prevents client-side script access to the cookie (security)
        options.Cookie.IsEssential = true; // Allows session to work even if cookie consent is required
    });
}

// =========================================================
// 4. MIDDLEWARE PIPELINE CONFIGURATION (USE SERVICES)
// =========================================================
void ConfigureMiddleware(WebApplication app)
{
    // Use exception handler only for non-development environments
    if (app.Environment.IsDevelopment())
    {
        // Used to show detailed database exceptions in development
        app.UseMigrationsEndPoint();
    }
    else
    {
        // Fallback page for exceptions in production
        app.UseExceptionHandler("/Home/Error");
        // Enforce Strict Transport Security (HSTS)
        app.UseHsts();
    }

    // Recommended Security Middleware Order

    // Redirect HTTP to HTTPS
    app.UseHttpsRedirection();

    // Serve static files (CSS, JS, images)
    app.UseStaticFiles();

    // Determines WHERE to route the request (must come before UseEndpoints)
    app.UseRouting();

    // Session must be used before UseAuthentication/UseAuthorization if session data affects security
    app.UseSession();

    // Authentication must run before Authorization
    app.UseAuthentication();

    // Enable authorization
    app.UseAuthorization();

    // Configure Custom Logging (must run after app.Build() to access services)
    ConfigureCustomLogging(app);

    // ====================== Endpoints (Routing) ======================
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    //app.MapControllerRoute(
    //    name: "default",
    //    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"
    //);

    app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();
}

// ====================== Role Seeding ======================

/// <summary>
/// Seeds the default Identity roles into the database on application startup.
/// </summary>
void SeedDatabaseRoles(WebApplication app)
{
    // Must use a scope to get services from the container after app is built
    using (var scope = app.Services.CreateScope())
    {
        var serviceProvider = scope.ServiceProvider;
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed the roles (using synchronous calls is acceptable here as it's a one-time startup task)
        // Check for customer role as a sentinel value
        if (!roleManager.RoleExistsAsync(AppRoles.Customer).GetAwaiter().GetResult())
        {
            Console.WriteLine("Seeding initial Identity roles...");

            roleManager.CreateAsync(new IdentityRole(AppRoles.Admin)).GetAwaiter().GetResult();
            roleManager.CreateAsync(new IdentityRole(AppRoles.Employee)).GetAwaiter().GetResult();
            roleManager.CreateAsync(new IdentityRole(AppRoles.Customer)).GetAwaiter().GetResult();

            Console.WriteLine("Identity roles successfully seeded.");
        }
    }
}

// ====================== Custom Logging Setup ======================
void ConfigureCustomLogging(WebApplication app)
{
    // Get Configuration from app.Configuration
    string formattedDate = DateTime.Now.ToString("MM-dd-yyyy");
    string baseLogPath = app.Configuration.GetValue<string>("Logging:LogFilePath") ?? "Logs";
    string logFilePath = Path.Combine(baseLogPath, $"log-{formattedDate}.txt");

    // Get required services from the application service provider
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

    // Add the custom logging provider
    loggerFactory.AddProvider(new CustomFileLoggerProvider(logFilePath, httpContextAccessor));
}
