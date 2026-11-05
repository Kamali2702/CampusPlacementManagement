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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // College code must be unique
        builder.Entity<College>()
            .HasIndex(c => c.CollegeCode)
            .IsUnique();

        // One user can have only one Placement Officer profile
        builder.Entity<PlacementOfficer>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        // Placement Officer -> Application User relationship
        builder.Entity<PlacementOfficer>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Placement Officer -> College relationship
        builder.Entity<PlacementOfficer>()
            .HasOne<College>()
            .WithMany()
            .HasForeignKey(p => p.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}