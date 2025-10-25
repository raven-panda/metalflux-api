using MetalfluxApi.Server.Authentication.Dto;
using MetalfluxApi.Server.Authentication.Service;
using Microsoft.AspNetCore.Mvc;

namespace MetalfluxApi.Server.Authentication;

[Route("auth")]
[ApiController]
public class AuthenticationController(IAuthService service, IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("register")]
    public IActionResult RegisterUser(RegisterDto dto)
    {
        var (createdUser, accessToken) = service.Register(dto, out var tokenExpiration);
        CreateTokenCookie(accessToken, tokenExpiration);

        return Ok(createdUser);
    }

    [HttpPost("login")]
    public IActionResult LoginUser(LoginDto dto)
    {
        var (createdUser, accessToken) = service.Login(dto, out var refreshTokenExpiration);
        CreateTokenCookie(accessToken, refreshTokenExpiration);

        return Ok(createdUser);
    }

    private void CreateTokenCookie(string token, DateTime tokenExpiration)
    {
        Response.Cookies.Append(
            configuration["Jwt:TokenCookieName"]!,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Expires = tokenExpiration,
                Secure = true,
                Path = "/",
            }
        );
    }
}
