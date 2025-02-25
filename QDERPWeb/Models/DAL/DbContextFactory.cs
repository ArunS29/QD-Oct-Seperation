using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Models.DAL
{
    public class DbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ERPMasterWtDataContext CreateDbContext(string connectionString)
        {
          //  connectionString = "Server=Lenovo;Database=ERP-MasterWtData;User Id=sa;Password=sql@123;TrustServerCertificate=True;";
            var optionsBuilder = new DbContextOptionsBuilder<ERPMasterWtDataContext>();
            optionsBuilder.UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
              .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            return new ERPMasterWtDataContext(optionsBuilder.Options);
        }



    }
}
