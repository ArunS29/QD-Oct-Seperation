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

            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(120); // Set timeout to 120 seconds
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(80),
                    errorNumbersToAdd: null
                );
            })
                        .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            return new ERPMasterWtDataContext(optionsBuilder.Options);
        }



    }
}
