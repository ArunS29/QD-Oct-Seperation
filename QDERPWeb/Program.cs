using Microsoft.EntityFrameworkCore;
using QDERPWeb.DAL.Entities;
using DevExpress.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var DBConnection = builder.Configuration.GetConnectionString("DBConnection");
// Add services to the container.
builder.Services
    .AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddDbContext<ERPMasterWtDataContext> (options =>
options.UseSqlServer(DBConnection));
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper registration
builder.Services.AddDevExpressControls();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/Finance"));
//app.MapControllers();
//app.MapDefaultControllerRoute();
app.MapFallbackToAreaPage("/AccountingLedgers", area: "Finance");

app.Run();
