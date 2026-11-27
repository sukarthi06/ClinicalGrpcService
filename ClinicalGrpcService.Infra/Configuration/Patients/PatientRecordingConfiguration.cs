namespace ClinicalGrpcService.Infra.Configuration.Patients;

public sealed class PatientRecordingConfiguration : IEntityTypeConfiguration<PatientRecording>
{
    public void Configure(EntityTypeBuilder<PatientRecording> builder)
    {
        builder.ToTable("PatientRecording", "patients");

        // One recording belongs to exactly one patient
        builder.HasKey(x => x.RecordingId);

        builder.Property(x => x.RecordingId)
            .HasConversion(id => id.Value, value => RecordingId.Of(value));

        builder.Property(x => x.PatientId)
            .HasConversion(id => id.Value, value => PatientId.Of(value));

        // Patients are only soft-deleted, so Restrict never blocks a normal delete
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // "All recordings for this patient"
        builder.HasIndex(x => x.PatientId);

        // Audit fields from Entity
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.LastModifiedAt);
        builder.Property(x => x.LastModifiedBy);
        builder.Property(x => x.RecordStatus).IsRequired();
    }
}