using System.ComponentModel.DataAnnotations;

namespace MetalfluxApi.Server.Modules.Media;

public class MediaModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FileExtension { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool HasUploadedMedia { get; set; } = false;

    public int TotalChunks { get; set; } = 0;

    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
