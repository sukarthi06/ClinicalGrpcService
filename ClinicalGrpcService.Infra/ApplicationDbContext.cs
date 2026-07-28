using ClinicalGrpcService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicalGrpcService.Infra;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<PhysicianNoteRecording> PhysicianNoteRecording { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
