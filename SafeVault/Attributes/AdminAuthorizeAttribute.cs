using Microsoft.AspNetCore.Authorization;

namespace SafeVault.Attributes;

/// <summary>
/// Authorization attribute that restricts access to admin users.
/// </summary>
public class AdminAuthorizeAttribute : AuthorizeAttribute
{
    public AdminAuthorizeAttribute()
    {
        Roles = Constants.Roles.Admin;
    }
}
