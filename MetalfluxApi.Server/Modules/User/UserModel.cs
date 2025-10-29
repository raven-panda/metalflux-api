using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalfluxApi.Server.Modules.User;

[Table("app_user")]
public class UserModel
{
    [Key]
    public long Id { get; set; }

    [Required, StringLength(30)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(255), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
