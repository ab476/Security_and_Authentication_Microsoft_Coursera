using System.Text.RegularExpressions;

namespace SafeVault;

public static partial class InputValidator
{
    private static readonly Regex UsernameRegex = UsernamePattern();

    private static readonly Regex EmailRegex = EmailPattern();

    private static readonly Regex ScriptTagRegex = ScriptPattern();

    public static bool IsSafeUsername(string username)
        => UsernameRegex.IsMatch(username) && !ContainsScript(username);

    public static bool IsSafeEmail(string email)
        => EmailRegex.IsMatch(email) && !ContainsScript(email);

    private static bool ContainsScript(string input)
        => ScriptTagRegex.IsMatch(input);

    [GeneratedRegex(@"^[a-zA-Z0-9_.-]{3,30}$", RegexOptions.Compiled)]
    private static partial Regex UsernamePattern();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"<script.*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "en-US")]
    private static partial Regex ScriptPattern();
}

