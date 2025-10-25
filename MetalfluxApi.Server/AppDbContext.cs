using MetalfluxApi.Server.Modules.Media;
using MetalfluxApi.Server.Modules.User;
using Microsoft.EntityFrameworkCore;

namespace MetalfluxApi.Server;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<MediaModel> Medias => Set<MediaModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserModel>().HasIndex(u => u.Email).IsUnique();
    }
}
