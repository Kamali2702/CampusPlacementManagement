using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<College> Colleges { get; set; }

    public DbSet<PlacementOfficer> PlacementOfficers { get; set; }

    public DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<College>()
            .HasIndex(c => c.CollegeCode)
            .IsUnique();

        builder.Entity<PlacementOfficer>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        builder.Entity<PlacementOfficer>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PlacementOfficer>()
            .HasOne<College>()
            .WithMany()
            .HasForeignKey(p => p.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Company>()
            .HasIndex(c => c.CompanyEmail)
            .IsUnique();
    }
}