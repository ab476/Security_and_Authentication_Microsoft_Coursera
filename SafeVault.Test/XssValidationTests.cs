namespace SafeVault.Test;

public class XssValidationTests
{
    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<ScRiPt>alert(1)</ScRiPt>")]
    [InlineData("<img src=x onerror=alert('boom')>")]
    [InlineData("<svg><script>malicious()</script>")]
    public void Username_Should_Fail_On_XSS_Attacks(string malicious)
    {
        var result = InputValidator.IsSafeUsername(malicious);
        Assert.False(result);
    }

    [Theory]
    [InlineData("<script>alert('hacked')</script>")]
    [InlineData("<iframe src='javascript:alert(2)'></iframe>")]
    public void Email_Should_Fail_On_XSS_Attacks(string malicious)
    {
        var result = InputValidator.IsSafeEmail(malicious);
        Assert.False(result);
    }
}
