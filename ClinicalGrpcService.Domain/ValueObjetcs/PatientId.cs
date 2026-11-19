namespace ClinicalGrpcService.Domain.ValueObjetcs;

public record PatientId
{
    public Guid Value { get; }
    private PatientId(Guid value) => Value = value;

    public static PatientId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PatientId cannot be empty.", nameof(value));
        }
        return new PatientId(value);
    }
}