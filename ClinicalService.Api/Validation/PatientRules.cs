namespace ClinicalService.Api.Validation;

public static class PatientRules
{
    // 10 digits, no country code. Formatting such as "(555) 123-4567" is allowed;
    // the normalizer keeps only the digits. A '+' (e.g. +91...) is rejected.
    public static IRuleBuilderOptions<T, string?> TenDigitPhone<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(BeTenDigitPhone)
            .WithMessage("Phone must be a 10-digit number without a country code.");

    private static bool BeTenDigitPhone(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || (value.All(c => char.IsAsciiDigit(c) || c is ' ' or '-' or '(' or ')' or '.')
            && value.Count(char.IsAsciiDigit) == 10);
}