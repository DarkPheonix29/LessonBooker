using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;


namespace LBRepository
{
	public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			// Look for appsettings.json in the startup project directory
			var config = new ConfigurationBuilder()
				.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LessonBooker"))
				.AddJsonFile("appsettings.json")
				.Build();

			var connectionString = config.GetConnectionString("LocalPostgres");

			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
			optionsBuilder.UseNpgsql(connectionString);

			return new ApplicationDbContext(optionsBuilder.Options);
		}
	}
}
