using ClinicalGrpcService.Domain.Enums;

namespace ClinicalGrpcService.Domain.Entities.Patients;

public class Patient : Entity
{
    public PatientId PatientId { get; set; } = PatientId.Of(Guid.NewGuid());
    public OrganizationId OrganizationId { get; set; } = default!;

    public string Name { get; set; } = default!;
    public DateOnly DoB { get; set; }
    public Sex? Sex { get; set; }
    public SexByBirth? SexByBirth { get; set; }
    public string? Title { get; set; }
    public string Mrn { get; set; } = default!;

    public string? Email { get; set; }
    public string? Phone { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }

    // Lookup hashes (HMAC); calculated by Infra, never set by callers   // new
    public string MrnHash { get; set; } = default!;                       // new
    public string? EmailHash { get; set; }                                // new
    public string? PhoneHash { get; set; }                                // new

    public List<PatientNameToken> NameTokens { get; set; } = [];         // new
}