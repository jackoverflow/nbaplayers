using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nbaplayers.Models;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<NBAPlayer> NBAPlayers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use the connection string from appsettings.json
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NBAPlayer>()
            .Property(p => p.DateOfBirth)
            .HasColumnType("date");
    }
}
