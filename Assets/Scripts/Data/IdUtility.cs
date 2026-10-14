using System.Globalization;
using System.Text;

public static class IdUtility
{
    // "Poción de Sueño" -> "pocion_de_sueno"
    public static string Sanitize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        string normalized = raw.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder(normalized.Length);

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue; //tildes y la ~ de la ñ

            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                sb.Append(c);
            else if ((c == ' ' || c == '_' || c == '-') && sb.Length > 0 && sb[sb.Length - 1] != '_')
                sb.Append('_');
        }

        return sb.ToString().TrimEnd('_');
    }

    public static bool IsValid(string id) => !string.IsNullOrEmpty(id) && id == Sanitize(id);
}