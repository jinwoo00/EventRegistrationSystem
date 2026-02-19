using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace EventRegistrationSystem.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // Add DbSet properties for your entities
    public DbSet<Event> Events { get; set; }
    public DbSet<Registration> Registrations { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships if needed
        builder.Entity<Registration>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Registration>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Attendance>()
            .HasOne(a => a.Registration)
            .WithOne()
            .HasForeignKey<Attendance>(a => a.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Certificate>()
            .HasOne(c => c.Event)
            .WithMany()
            .HasForeignKey(c => c.EventId);

        builder.Entity<Certificate>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);
    }
}
