using GastAi.Contracts.Auth;
using GastAi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GastAi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Email, request.Password, isPersistent: true, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Unauthorized(new ProblemDetails { Title = "Conta temporariamente bloqueada após várias tentativas inválidas.", Status = 401 });

        if (!result.Succeeded)
            return Unauthorized(new ProblemDetails { Title = "E-mail ou senha inválidos.", Status = 401 });

        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
    {
        var email = User.Identity?.Name;
        return email is null ? Unauthorized() : Ok(new MeResponse(email));
    }
}
