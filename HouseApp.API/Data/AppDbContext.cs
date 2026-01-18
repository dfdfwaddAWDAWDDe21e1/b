using Microsoft.EntityFrameworkCore;
using HouseApp.API.Models;

namespace HouseApp.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<House> Houses { get; set; }
    public DbSet<HouseTenant> HouseTenants { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
        });

        // House entity
        modelBuilder.Entity<House>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.MonthlyRent).HasPrecision(18, 2);
            entity.Property(e => e.UtilitiesCost).HasPrecision(18, 2);
            entity.Property(e => e.WaterBillCost).HasPrecision(18, 2);

            entity.HasOne(e => e.Landlord)
                .WithMany(u => u.Houses)
                .HasForeignKey(e => e.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // HouseTenant entity
        modelBuilder.Entity<HouseTenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.HouseId, e.StudentId });

            entity.HasOne(e => e.House)
                .WithMany(h => h.HouseTenants)
                .HasForeignKey(e => e.HouseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany(u => u.HouseTenants)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.Student)
                .WithMany(u => u.Payments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.House)
                .WithMany(h => h.Payments)
                .HasForeignKey(e => e.HouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MessageText).IsRequired();
            entity.Property(e => e.SenderName).IsRequired();

            entity.HasOne(e => e.House)
                .WithMany(h => h.Messages)
                .HasForeignKey(e => e.HouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
