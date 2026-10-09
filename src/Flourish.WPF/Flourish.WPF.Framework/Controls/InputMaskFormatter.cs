namespace ArkheideSystem.Flourish.WPF.Controls;

internal static class InputMaskFormatter
{
    public static string Normalize(string mask, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mask);
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var candidates = value.Where(IsAsciiLetterOrDigit).ToArray();
        var result = new char[mask.Count(IsSlot)];
        var resultIndex = 0;
        var candidateIndex = 0;

        foreach (var slot in mask.Where(IsSlot))
        {
            while (candidateIndex < candidates.Length)
            {
                var candidate = candidates[candidateIndex++];
                if (slot == '0' && !IsAsciiDigit(candidate)) continue;
                result[resultIndex++] = slot == 'A'
                    ? char.ToUpperInvariant(candidate)
                    : candidate;
                break;
            }
        }

        return new string(result, 0, resultIndex);
    }

    public static string Format(string mask, string? value)
    {
        var normalized = Normalize(mask, value);
        if (normalized.Length == 0) return string.Empty;

        var formatted = new List<char>(mask.Length);
        var valueIndex = 0;
        foreach (var maskCharacter in mask)
        {
            if (IsSlot(maskCharacter))
            {
                if (valueIndex >= normalized.Length) break;
                formatted.Add(normalized[valueIndex++]);
            }
            else if (valueIndex > 0)
            {
                formatted.Add(maskCharacter);
            }
        }

        return new string([.. formatted]);
    }

    public static bool IsSlot(char value) => value is '0' or 'A';

    private static bool IsAsciiLetterOrDigit(char value) =>
        IsAsciiDigit(value) || value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool IsAsciiDigit(char value) => value is >= '0' and <= '9';
}
