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
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retry attempts
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Maximum delay between retries
                    errorNumbersToAdd: null // Optional: specify SQL error codes to retry on
                );
            })
                        .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            return new ERPMasterWtDataContext(optionsBuilder.Options);
        }



    }
}
