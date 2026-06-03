using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CareLink.Persistence.Factories
{
    public class CareLinkDbContextFactory : IDesignTimeDbContextFactory<CareLinkDbContext>
    {
        public CareLinkDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CareLinkDbContext>();

            string connection = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connection);

            return new CareLinkDbContext(optionsBuilder.Options);
        }
    }
}