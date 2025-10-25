using System.ComponentModel.DataAnnotations;

namespace MetalfluxApi.Server.Authentication.Dto;

public class RegisterDto
{
    [Required, StringLength(30)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(255), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$")]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$")]
    public string PasswordConfirm { get; set; } = string.Empty;
}
