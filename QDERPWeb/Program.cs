using QD.ERP.Web;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DALCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using SaasKit.Multitenancy;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using DevExpress.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using DevExpress.AspNetCore.Reporting;
using Microsoft.EntityFrameworkCore.Internal;
using QD.ERP.Web.Service;
using Serilog;
using Serilog.Events;
using Microsoft.ApplicationInsights.Extensibility;
using QD.ERP.Web.Service.ReportService;
using QD.ERP.Web.Middlewares;
using DevExpress.XtraCharts;

var builder = WebApplication.CreateBuilder(args);

#region **1. Configure Services**

// **1.1 Add DevExpress Reporting Services**
builder.Services.AddDevExpressControls();
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddScoped<DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension, ReportStorageWebExtension>();
builder.Services.ConfigureReportingServices(configurator =>
{
    configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
    {
        viewerConfigurator.UseCachedReportSourceBuilder();
    });
});

var DBConnection = builder.Configuration.GetConnectionString("DBConnection");
builder.Services.AddDbContext<QD.ERP.Web.DAL.Entities.ERPMasterWtDataContext>(options =>
    options.UseSqlServer(DBConnection));

var CommonDBConnection = builder.Configuration.GetConnectionString("CommonDBConnection");
builder.Services.AddDbContext<ERPCommonContext>(options =>
    options.UseSqlServer(CommonDBConnection));

builder.Services
    .AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.Add(new TenantRouteModelConvention());
});

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
app.UseRouting();

app.Use(async (context, next) =>
{
    var tenantName = context.GetRouteValue("tenantName")?.ToString();
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseMiddleware<TokenValidationMiddleware>();
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

app.MapGet("/", () => Results.Redirect("/Pulse/Security/Login"));

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
