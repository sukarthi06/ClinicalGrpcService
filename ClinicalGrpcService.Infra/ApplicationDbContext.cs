namespace ClinicalGrpcService.Infra;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IFieldEncryptor encryptor) : DbContext(options)
{
    public DbSet<PhysicianNoteRecording> PhysicianNoteRecording { get; set; } = default!;
    public DbSet<Patient> Patients { get; set; } = default!;
    public DbSet<PatientRecording> PatientRecordings { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new PatientConfiguration(encryptor));
    }
}