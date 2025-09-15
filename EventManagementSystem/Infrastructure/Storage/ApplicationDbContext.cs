using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Infrastructure.Storage;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // This constructor is ESSENTIAL
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // CRITICAL: Must be first

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Creator)        // An Event has one Creator
            .WithMany()                    // A Creator has many Events (but we won't navigate from User->Events here)
            .HasForeignKey(e => e.CreatedBy) // Foreign key
            .OnDelete(DeleteBehavior.Restrict); // Delete user -> doesn't delete their events

        modelBuilder.Entity<Event>()
            .HasIndex(e => new { e.Name, e.StartTime, e.Location })
            .IsUnique();


        modelBuilder.Entity<Registration>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Registration>()
            .HasIndex(r => new { r.EventId, r.Email });
    }
}