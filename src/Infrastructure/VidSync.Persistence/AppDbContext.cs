using Microsoft.EntityFrameworkCore;
using VidSync.Domain.Entities;

public class AppDbContext : DbContext
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
            entity.Property(e => e.Username).IsRequired(true).HasMaxLength(170);
            entity.Property(e => e.FirstName).IsRequired(true).HasMaxLength(50);
            entity.Property(e => e.MiddleName).IsRequired(false).HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired(true).HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired(true).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired(true).HasMaxLength(256);
            entity.Property(e => e.ProfilePictureUrl).IsRequired(false).HasMaxLength(256).HasDefaultValue("https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740");
            entity.Property(e => e.CreatedAt).IsRequired(true).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
        });
    }
}
