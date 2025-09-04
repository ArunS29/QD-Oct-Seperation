using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.Spreadsheet.Charts;
using DevExpress.XtraCharts;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using QD.ERP.Web;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Middleware;
using QD.ERP.Web.Middlewares;
using QD.ERP.Web.Models.DALCommon;
using QD.ERP.Web.Service;
using QD.ERP.Web.Service.ReportService;
using QD.ERP.Web.Services.Logging;
using SaasKit.Multitenancy;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2; 


if (FirebaseApp.DefaultInstance == null)
{

    var path = Path.Combine(AppContext.BaseDirectory, "firebase-config.json");

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(path)
    });

    Debug.WriteLine("✅ Firebase initialized with app name: " + FirebaseApp.DefaultInstance.Name);
}


var builder = WebApplication.CreateBuilder(args);


// builder.WebHost.UseUrls("http://*:44300");

#region **1. Configure Services**

// **1.1 Add DevExpress Reporting Services**
builder.Services.AddDevExpressControls();

// Registering ReportDbContext with tenant-specific connection string
builder.Services.AddScoped<ReportDbContext>(serviceProvider =>
{
    var tenantDbContextHelper = serviceProvider.GetRequiredService<TenantDbContextHelper>();

    if (tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out _))
    {
        // Use tenant's connection string dynamically
        var options = new DbContextOptionsBuilder<ReportDbContext>()
            .UseSqlServer(tenant.ConnectionString) // Using tenant's connection string here
            .Options;

        return new ReportDbContext(options, tenant.ConnectionString); // Passing connection string to the constructor
    }

    throw new InvalidOperationException("Tenant or DbContext could not be resolved.");
});

builder.Services.AddScoped<DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension, ReportStorageWebExtension>();

builder.Services.AddScoped<FcmService>();

// Configuring Reporting Services
builder.Services.ConfigureReportingServices(configurator =>
{
    configurator.ConfigureReportDesigner(designerConfigurator =>
    {
        // Correct method to allow custom SQL queries
        designerConfigurator.EnableCustomSql();
    });
    configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
    {
        viewerConfigurator.UseCachedReportSourceBuilder();
    });
});

var CommonDBConnection = builder.Configuration.GetConnectionString("CommonDBConnection");
builder.Services.AddDbContext<ERPCommonContext>(options =>
    options.UseSqlServer(CommonDBConnection).EnableSensitiveDataLogging());
builder.Services.AddDbContext<ERPMasterWtDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.Add(new TenantRouteModelConvention());
});
builder.Services.AddScoped<LicenseService>();

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout to 1 hour
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserActionLogger, UserActionLogger>();
builder.Services.AddScoped<DbContextFactory>();
builder.Services.AddMultitenancy<Tenant, TenantResolver>();
builder.Services.AddScoped<UserAccessService>();
builder.Services.AddScoped<CurrencyService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<TenantDbContextHelper>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<AttachmentCleanupService>();
builder.Services.AddAutoMapper(typeof(Program));

var excludedAreas = new[] { "Security", "Help" };
builder.Services.AddSingleton<IEnumerable<string>>(excludedAreas);

builder.Services.AddAuthorization();
System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

// Configure Serilog
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console();

// Load local configuration from appsettings.json
IConfiguration localConfig = builder.Configuration;

// Get Azure App Configuration connection string from appsettings.json
string azureConnectionString = localConfig["AzureAppConfig:ConnectionString"];

// Initialize the ConfigurationHelper
var configurationHelper = new ConfigurationHelper(localConfig, azureConnectionString);

// Example: Access configuration values
string mySetting = configurationHelper.GetConfigurationValue("MySetting");
Console.WriteLine($"MySetting Value: {mySetting}");


//var containerUri = configurationHelper.GetConfigurationValue("AzureBlobStorage:ClientFilesContainerUri");
//if (string.IsNullOrWhiteSpace(containerUri))
//{
//    throw new Exception("AzureBlobStorage:ClientFilesContainerUri is missing in configuration.");
//}

builder.Services.AddSingleton<ClientFilesStorageHelper>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<ClientFilesStorageHelper>>();
    var config = provider.GetRequiredService<IConfiguration>();

    var containerUri = config["AzureBlobStorage:ClientFilesContainerUri"];
    var connectionString = config["AzureBlobStorage:ClientFilesConnectionString"];
    var containerName = config["AzureBlobStorage:ClientFilesContainerName"];

    if (string.IsNullOrEmpty(containerUri))
        throw new Exception("AzureBlobStorage:ClientFilesContainerUri is missing.");

    return new ClientFilesStorageHelper(containerUri, connectionString, containerName, logger);
});


builder.Services.AddSingleton(new OpenAIChatService(
    "https://applicationchatbot.cognitiveservices.azure.com/",
    "gpt-4.1",
    "6BdBUxBd1DoluRRPr3vutKBb6zgDMy0yE4f4tptXOGqLUH120UvoJQQJ99BGACYeBjFXJ3w3AAAAACOGczCL"
));

if (builder.Environment.IsDevelopment())
{
    loggerConfiguration.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
}
else
{
    loggerConfiguration.WriteTo.ApplicationInsights(new TelemetryConfiguration
    {
        InstrumentationKey = builder.Configuration["ApplicationInsights:InstrumentationKey"]
    }, TelemetryConverter.Traces);
}

Log.Logger = loggerConfiguration.CreateLogger();
builder.Host.UseSerilog();

#endregion

var app = builder.Build();

#region **2. Configure Middleware**

app.UseDevExpressControls();
//app.UseMiddleware<LicenseValidationMiddleware>(); // Add before UseRouting
app.UseRouting();
//app.UseStatusCodePages("text/plain", "Status Code: {0}");
//app.UseStatusCodePagesWithRedirects("/Error/{0}");
//app.Use(async (context, next) =>
//{
//    await next();

//    if (context.Response.StatusCode == 404)
//    {
//        context.Response.Redirect("/Error/404");
//    }
//});

app.UseSession();

app.Use(async (context, next) =>
{
    string tenantName = null;

    // ✅ 1. Try from route value
    tenantName = context.GetRouteValue("tenantName")?.ToString();

    // ✅ 2. If not found, try from body (for POST/PUT/PATCH with JSON)
    if (string.IsNullOrEmpty(tenantName) &&
        (context.Request.Method == HttpMethods.Post ||
         context.Request.Method == HttpMethods.Put ||
         context.Request.Method == HttpMethods.Patch) &&
        context.Request.ContentType != null &&
        context.Request.ContentType.Contains("application/json"))
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                if (json != null && json.TryGetValue("TenantName", out var tn))
                {
                    tenantName = tn?.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching Tenant Name: {ex.Message}");
            }
        }
    }

    // ✅ 3. If still not found, try from session
    if (string.IsNullOrEmpty(tenantName))
    {
        tenantName = context.User?.FindFirst("TenantName")?.Value;
        //tenantName = context.Session.GetString("TenantName");
        Debug.WriteLine($"Session TenantName: {tenantName}");
    }

    // ✅ Store for the rest of the request
    if (!string.IsNullOrEmpty(tenantName))
    {
        context.Items["TenantName"] = tenantName;
    }

    await next();
});


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
// Use exception handling for non-development environments
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Test/Finance/Error");
//    app.UseStatusCodePagesWithReExecute("/Test/Finance/PageNotFound");
//}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<TenantSessionMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseMiddleware<TokenRenewalMiddleware>();
app.UseMultitenancy<Tenant>();
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandler>();

// Initialize EmailHelper with the correct configuration
QD.ERP.Web.Areas.Utility.EmailHelper.Initialize(app.Configuration);

#endregion

#region **3. Configure Routing**

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", () => Results.Redirect("/Aicon/security/login"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.MapRazorPages();

#endregion

app.Run();

#region **5. Custom Route Model Convention for Razor Pages**

public class TenantRouteModelConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        foreach (var selector in model.Selectors)
        {
            var attributeRouteModel = selector.AttributeRouteModel;
            if (attributeRouteModel != null)
            {
                attributeRouteModel.Template = "{tenantName}/" + attributeRouteModel.Template;
            }
        }
    }
}

#endregion