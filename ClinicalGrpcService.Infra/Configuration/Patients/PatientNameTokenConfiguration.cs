namespace ClinicalGrpcService.Infra.Configuration.Patients;

public sealed class PatientNameTokenConfiguration : IEntityTypeConfiguration<PatientNameToken>
{
    public void Configure(EntityTypeBuilder<PatientNameToken> builder)
    {
        builder.ToTable("PatientNameToken", "patients");

        // One key serves name search (OrganizationId, TokenHash) and per-patient cleanup
        builder.HasKey(x => new { x.OrganizationId, x.TokenHash, x.PatientId });

        builder.Property(x => x.OrganizationId)
            .HasConversion(id => id.Value, value => OrganizationId.Of(value));

        builder.Property(x => x.PatientId)
            .HasConversion(id => id.Value, value => PatientId.Of(value));

        builder.Property(x => x.TokenHash).HasMaxLength(64);
    }
}