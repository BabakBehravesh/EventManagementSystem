using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Infrastructure.Storage;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Participation> Participations => Set<Participation>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Creator)        
            .WithMany()                    
            .HasForeignKey(e => e.CreatedBy) 
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Event>()
            .HasIndex(e => new { e.Name, e.StartTime, e.Location })
            .IsUnique();


        modelBuilder.Entity<Participation>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Participation>()
            .HasIndex(r => new { r.EventId, r.Email })
            .IsUnique();
    }
}