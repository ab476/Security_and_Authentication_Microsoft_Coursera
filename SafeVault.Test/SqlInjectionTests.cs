namespace SafeVault.Test;

public class SqlInjectionTests
{
    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("admin'; DROP TABLE Users; --")]
    [InlineData("\" OR \"\" = \"")]
    [InlineData("'; SHUTDOWN --")]
    public void Username_Should_Fail_On_SQL_Injection(string malicious)
    {
        // Act
        bool isSafe = InputValidator.IsSafeUsername(malicious);

        // Assert
        Assert.False(isSafe);
    }

    [Theory]
    [InlineData("test@example.com'; DROP TABLE Users; --")]
    [InlineData("' OR '' = '")]
    public void Email_Should_Fail_On_SQL_Injection(string malicious)
    {
        // Act
        bool isSafe = InputValidator.IsSafeEmail(malicious);

        // Assert
        Assert.False(isSafe);
    }
}

