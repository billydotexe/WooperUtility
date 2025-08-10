using Microsoft.EntityFrameworkCore;

namespace WooperUtility.Datacontext;

public class WooperContext : DbContext
{
	public DbSet<User> Users {get; set;}
	public DbSet<Admin> Admins {get; set;}
	public DbSet<BannedUser> BannedUsers {get; set;}
	public DbSet<BotRequest> Requests {get; set;}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.Build();
		optionsBuilder.UseSqlite(configuration.GetConnectionString("WooperDb"));

	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		ArgumentNullException.ThrowIfNull(modelBuilder);

		modelBuilder.Entity<BotRequest>()
			.HasOne(x => x.User);

		modelBuilder.Entity<Admin>()
			.HasOne(x => x.User);

		modelBuilder.Entity<BannedUser>()
			.HasOne(x => x.User);
	}
}
