using System.Globalization;
using System.Text;

namespace ClinicalGrpcService.Infra.Security;

public static class LookupNormalizer
{
    private static readonly char[] NameSeparators = [' ', '\t', ',', '.', '-'];

    public static string? Mrn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim().ToUpperInvariant();
    }

    public static string? Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim().ToLowerInvariant();
    }

    public static string? Phone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        // Input validation guarantees a 10-digit number without country code
        var digits = new string(value.Where(char.IsAsciiDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    public static IReadOnlyCollection<string> NameTokens(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        return RemoveDiacritics(value)
            // Keep apostrophes as entered; unify typographic variants (U+2019, U+2018, U+02BC) to a plain '
            .Replace('\u2019', '\'').Replace('\u2018', '\'').Replace('\u02BC', '\'')
            .ToUpperInvariant()
            .Split(NameSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();
    }

    private static string RemoveDiacritics(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}