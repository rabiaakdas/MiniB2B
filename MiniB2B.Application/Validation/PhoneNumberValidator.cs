using System.Text.RegularExpressions;

namespace MiniB2B.Application.Validation;

public static partial class PhoneNumberValidator
{
    public const string ErrorMessage = "Telefon numarası 05 ile başlamalı ve 11 haneli olmalıdır.";

    public static bool IsValidOptionalPhoneNumber(string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber)
            || TurkishMobilePhoneRegex().IsMatch(phoneNumber.Trim());
    }

    [GeneratedRegex("^05\\d{9}$")]
    private static partial Regex TurkishMobilePhoneRegex();
}
