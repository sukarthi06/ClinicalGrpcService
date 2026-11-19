namespace ClinicalGrpcService.Domain.Enums;

// Sex assigned at birth (based on USCDI "Sex")
public enum SexByBirth
{
    Unknown = 0,
    Male = 1,
    Female = 2,
    Intersex = 3,
    AskedButDeclined = 4
}