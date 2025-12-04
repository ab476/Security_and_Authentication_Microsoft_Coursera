using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    ITokenService tokenService) : ControllerBase
{

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
            return Ok();

        string errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return BadRequest(errors);
    }

    /// <summary>
    /// Logs a user in and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        user ??= await userManager.FindByEmailAsync(request.Username);

        if (user is null)
            return Unauthorized();

        var passwordCheck = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!passwordCheck.Succeeded)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        string token = await tokenService.GenerateJwtTokenAsync(user, roles);

        return Ok(token);
    }
}


