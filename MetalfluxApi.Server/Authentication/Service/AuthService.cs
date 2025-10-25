using MetalfluxApi.Server.Authentication.Dto;
using MetalfluxApi.Server.Core.Exceptions;
using MetalfluxApi.Server.Modules.User;
using Microsoft.IdentityModel.Tokens;

namespace MetalfluxApi.Server.Authentication.Service;

public interface IAuthService
{
    (UserDto createdUser, string accessToken) Register(
        RegisterDto dto,
        out DateTime tokenExpiration
    );
    (UserDto createdUser, string accessToken) Login(LoginDto dto, out DateTime tokenExpiration);
}

internal sealed class AuthService(
    IUserService service,
    IUserRepository repo,
    TokenProvider tokenProvider,
    IPasswordHasher passwordHasher,
    IConfiguration configuration
) : IAuthService
{
    public (UserDto createdUser, string accessToken) Register(
        RegisterDto dto,
        out DateTime tokenExpiration
    )
    {
        if (dto.Password != dto.PasswordConfirm)
            throw new PasswordsNotMatchingException();

        var userModel = service.ToModel(new UserDto { Email = dto.Email, Username = dto.Username });
        if (repo.ExistsByEmail(userModel.Email))
            throw new EntityUniqueConstraintViolationException("User", "Email");

        userModel.Password = passwordHasher.Hash(dto.Password);

        var userDto = service.ToDto(repo.Add(userModel));

        var accessToken = tokenProvider.GenerateAccessToken(userDto, out tokenExpiration);
        return (userDto, accessToken);
    }

    public (UserDto createdUser, string accessToken) Login(
        LoginDto dto,
        out DateTime refreshTokenExpiration
    )
    {
        var user = repo.GetByEmail(dto.Email);
        if (user is null || !passwordHasher.VerifyHashedPassword(user.Password, dto.Password))
            throw new InvalidCredentialsException();
        var userDto = service.ToDto(user);

        var accessToken = tokenProvider.GenerateAccessToken(userDto, out refreshTokenExpiration);
        return (userDto, accessToken);
    }
}
