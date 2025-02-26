using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace QD.ERP.Web
{
    // Define custom data connection types
    public class SqlDataConnectionDescription : DataConnection { }
    public class JsonDataConnectionDescription : DataConnection { }

    // Abstract class for data connection
    public abstract class DataConnection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ConnectionString { get; set; }
    }

    // Class for storing report items
    public class ReportItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public byte[] LayoutData { get; set; }
    }

    // Class for mapping to tbl90112ReportAttributes
    public class ReportAttribute
    {
        // Assuming 'ReportNo' is the primary key
        public string ReportNo { get; set; }
        public string ReportName { get; set; } // This is the ReportName from your database table
        public string ReportDescription { get; set; } // Assuming you want this to map to the Description column
        public byte[] ReportXML { get; set; }  // Layout data
    }

    public class ReportDbContext : DbContext
    {
        public DbSet<ReportItem> Reports { get; set; }
        public DbSet<DataConnection> DataConnections { get; set; }
        public DbSet<ReportAttribute> ReportAttributes { get; set; }

        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
        {
        }

        public void InitializeDatabase()
        {
            Database.Migrate(); // Apply pending migrations to ensure the schema is up to date
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataConnection>()
                .HasDiscriminator<string>("ConnectionType")
                .HasValue<SqlDataConnectionDescription>("Sql")
                .HasValue<JsonDataConnectionDescription>("Json");

            // Map the ReportAttribute entity to the tbl90112ReportAttributes table
            modelBuilder.Entity<ReportAttribute>()
                .ToTable("tbl90112ReportAttributes");

            // Define ReportNo as the primary key
            modelBuilder.Entity<ReportAttribute>()
                .HasKey(r => r.ReportNo);

            // Additional configuration can be done for other entities if needed
        }
    }


 
}
