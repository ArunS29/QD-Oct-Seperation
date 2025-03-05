using QD.ERP.Web;
using QD.ERP.Web.DAL.Entities;
//using QD.ERP.Web.Models.DAL;
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
using QD.ERP.Web.ReportService;

var builder = WebApplication.CreateBuilder(args);

#region **1. Configure Services**

// **1.1 Add DevExpress Reporting Services**
// Add services for Reporting
builder.Services.AddDevExpressControls();

// Register DbContext for the ReportDbContext
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"))); // Make sure the connection string is correct

// Register ReportStorageWebExtension to use ReportDbContext
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

// **1.3 Add Razor Pages and JSON Configuration**
builder.Services
    .AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// **1.4 Razor Page Routing Convention for Multitenancy**
builder.Services.AddRazorPages(options =>
{
    options.Conventions.Add(new TenantRouteModelConvention());
});

// **1.5 Add Caching, Multitenancy, and Other Dependencies**
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
builder.Services.AddScoped<TenantDbContextHelper>(); // Register TenantDbContextHelper
builder.Services.AddHttpClient();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper registration

// **1.6 Define Excluded Areas**
var excludedAreas = new[] { "Security", "Help" };
builder.Services.AddSingleton<IEnumerable<string>>(excludedAreas);

// **1.7 Add Authorization and Security**
builder.Services.AddAuthorization();
System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

#endregion

var app = builder.Build();

#region **2. Configure Middleware**

// **2.1 Enable DevExpress Controls**
app.UseDevExpressControls();

// **2.2 Configure Routing Middleware (Tenant Extraction)**
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

// **2.3 Exception Handling for Production**
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// **2.4 Enable Security & Authentication Middleware**
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession(); // ✅ Fix: Move after UseRouting
app.UseMiddleware<TokenValidationMiddleware>();
app.UseMultitenancy<Tenant>();
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region **3. Configure Routing**

// **3.1 Define Area-Based Routing**
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// **3.2 Default Routing**
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// **3.3 Redirect Root URL to Login Page**
app.MapGet("/", () => Results.Redirect("/Pulse/Security/Login"));

// **3.4 Define Login Controller Routing**
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

// **3.5 Enable Razor Pages**
app.MapRazorPages();

#endregion

// **4. Run the Application**
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
