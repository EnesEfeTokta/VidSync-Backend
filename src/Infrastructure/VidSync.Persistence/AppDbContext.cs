using Microsoft.EntityFrameworkCore;
using VidSync.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace VidSync.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired(true).HasMaxLength(50);
            entity.Property(e => e.MiddleName).IsRequired(false).HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired(true).HasMaxLength(50);
            entity.Property(e => e.ProfilePictureUrl).IsRequired(false).HasMaxLength(256).HasDefaultValue("https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740");
            entity.Property(e => e.CreatedAt).IsRequired(true).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
        });
    }
}
