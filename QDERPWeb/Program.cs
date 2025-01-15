using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using DevExpress.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var DBConnection = builder.Configuration.GetConnectionString("DBConnection");
// Add services to the container.
builder.Services
    .AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddDbContext<ERPMasterWtDataContext>(options =>
options.UseSqlServer(DBConnection));
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper registration
builder.Services.AddDevExpressControls();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Redirect to login if unauthenticated
        options.AccessDeniedPath = "/AccessDenied"; // Redirect if unauthorized
    });
var app = builder.Build();

// Add Authorization
builder.Services.AddAuthorization();

// Ensure secure communication with TLS 1.2
System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();
// Enable HTTPS redirection
app.UseHttpsRedirection();

app.UseRouting();
// Enable Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseAuthorization();
// Map the Login page as the root/default route
app.MapGet("/", () => Results.Redirect("/Login"));

// Default controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

// Razor Pages
app.MapRazorPages();
app.Run();