using KS_Sweets.Application.CrossCuttingConcerns.Logging;
using KS_Sweets.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure services for dependency injection
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigureMiddleware(app);

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Configure database context
    ConfigureDatabase(services, configuration);

    // Add services to the container.
    services.AddControllersWithViews();

    // Add HTTP context accessor
    services.AddHttpContextAccessor();
}
void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("KSSweetsDbConnection");

    // Add DbContext with SQL Server
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
void ConfigureMiddleware(WebApplication app)
{
    // Redirect HTTP to HTTPS
    app.UseHttpsRedirection();

    // Apply CORS policy
    app.UseCors("AllowMyOrigin");

    // Serve static files
    app.UseStaticFiles();

    // Enable authentication
    app.UseAuthentication();

    // Enable authorization
    app.UseAuthorization();

    // Map controller endpoints
    app.MapControllerRoute(
     name: "default",
     pattern: "{controller=Home}/{action=Index}/{id?}");

    // Configure Logging
    ConfigureCustomLogging(app);
}

void ConfigureCustomLogging(WebApplication app)
{
    string formattedDate = DateTime.Now.ToString("MM-dd-yyyy");
    string baseLogPath = builder.Configuration.GetValue<string>("Logging:LogFilePath");
    string logFilePath = Path.Combine(baseLogPath, $"log-{formattedDate}.txt");

    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    loggerFactory.AddProvider(new CustomFileLoggerProvider(logFilePath, httpContextAccessor));
}