using System.Text.RegularExpressions;

namespace MiniB2B.Application.Validation;

public static partial class EmailValidator
{
    public const string ErrorMessage = "Geçerli bir e-posta adresi giriniz.";

    public static bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email)
            && EmailRegex().IsMatch(email.Trim());
    }

    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    private static partial Regex EmailRegex();
}
