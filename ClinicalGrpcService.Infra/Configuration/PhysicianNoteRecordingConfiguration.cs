using ClinicalGrpcService.Domain.Entities;
using ClinicalGrpcService.Domain.ValueObjetcs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicalGrpcService.Infra.Configuration;

public sealed class PhysicianNoteRecordingConfiguration
    : IEntityTypeConfiguration<PhysicianNoteRecording>
{
    public void Configure(EntityTypeBuilder<PhysicianNoteRecording> builder)
    {
        builder.ToTable("PhysicianNoteRecording", "notes");

        builder.HasKey(x => x.PhysicianNoteId);

        builder.Property(x => x.PhysicianNoteId)
            .HasConversion(
                id => id.Value,
                value => PhysicianNoteId.Of(value));

        builder.Property(x => x.RecordingId)
            .HasConversion(
                id => id.Value,
                value => RecordingId.Of(value));

        builder.Property(x => x.StoragePath)
            .HasMaxLength(500);

        // Inherited properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy);

        builder.Property(x => x.RecordStatus)
            .IsRequired();
    }
}
