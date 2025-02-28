using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.DAL.Entities
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
            var optionsBuilder = new DbContextOptionsBuilder<ERPMasterWtDataContext>();
            optionsBuilder.UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            return new ERPMasterWtDataContext(optionsBuilder.Options);
        }



    }
}
