using Microsoft.EntityFrameworkCore;
using VidSync.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using VidSync.Domain.Interfaces;
using VidSync.Persistence.Converters;

namespace VidSync.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly ICryptoService _cryptoService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICryptoService cryptoService)
        : base(options)
    {
        _cryptoService = cryptoService;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var encryptedConverter = new EncryptedStringConverter(_cryptoService);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired(true).HasMaxLength(50);
            entity.Property(e => e.MiddleName).IsRequired(false).HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired(true).HasMaxLength(50);
            entity.Property(e => e.ProfilePictureUrl).IsRequired(false).HasMaxLength(256).HasDefaultValue("https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740");
            entity.Property(e => e.CreatedAt).IsRequired(true).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Rooms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired(true).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired(true).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ExpiresAt).IsRequired(false).HasColumnType("timestamp with time zone");
            entity.Property(e => e.IsActive).IsRequired(true).HasDefaultValue(true);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired(true).HasColumnType("text").HasConversion(encryptedConverter);
            entity.Property(e => e.SentAt).IsRequired(true).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Room)
                  .WithMany()
                  .HasForeignKey(e => e.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SentAt);
        });
    }
}
