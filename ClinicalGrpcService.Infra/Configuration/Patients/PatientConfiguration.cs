using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClinicalGrpcService.Infra.Configuration.Patients;

public sealed class PatientConfiguration(IFieldEncryptor encryptor) : IEntityTypeConfiguration<Patient>
{
    private const string DateFormat = "yyyy-MM-dd";

    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        var encrypted = new ValueConverter<string, string>(
            v => encryptor.Encrypt(v),
            v => encryptor.Decrypt(v));

        var encryptedDate = new ValueConverter<DateOnly, string>(
            d => encryptor.Encrypt(d.ToString(DateFormat, CultureInfo.InvariantCulture)),
            s => DateOnly.ParseExact(encryptor.Decrypt(s), DateFormat, CultureInfo.InvariantCulture));

        builder.ToTable("Patient", "patients");

        builder.HasKey(x => x.PatientId);

        builder.Property(x => x.PatientId)
            .HasConversion(id => id.Value, value => PatientId.Of(value));

        builder.Property(x => x.OrganizationId)
            .HasConversion(id => id.Value, value => OrganizationId.Of(value))
            .IsRequired();

        // Encrypted (AES); stored as text, no max length
        builder.Property(x => x.Name).HasConversion(encrypted).IsRequired();
        builder.Property(x => x.DoB).HasConversion(encryptedDate).IsRequired();
        builder.Property(x => x.Mrn).HasColumnName("MRN").HasConversion(encrypted).IsRequired();
        builder.Property(x => x.Email).HasConversion(encrypted!);
        builder.Property(x => x.Phone).HasConversion(encrypted!);
        builder.Property(x => x.AddressLine1).HasConversion(encrypted!);
        builder.Property(x => x.AddressLine2).HasConversion(encrypted!);
        builder.Property(x => x.City).HasConversion(encrypted!);
        builder.Property(x => x.Zip).HasConversion(encrypted!);

        // Plain
        builder.Property(x => x.Sex).HasConversion<short>();
        builder.Property(x => x.SexByBirth).HasConversion<short>();
        builder.Property(x => x.Title).HasMaxLength(20);
        builder.Property(x => x.State).HasMaxLength(50);

        // Lookup hashes (HMAC)
        builder.Property(x => x.MrnHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EmailHash).HasMaxLength(64);
        builder.Property(x => x.PhoneHash).HasMaxLength(64);

        // An MRN is unique within an organization (includes inactive patients, so they're reactivated)
        builder.HasIndex(x => new { x.OrganizationId, x.MrnHash }).IsUnique();

        // Name search tokens
        builder.HasMany(x => x.NameTokens)
            .WithOne()
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inherited properties
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy);
        builder.Property(x => x.LastModifiedAt);
        builder.Property(x => x.LastModifiedBy);
        builder.Property(x => x.RecordStatus).IsRequired();
    }
}