using System.ComponentModel.DataAnnotations;

namespace MetalfluxApi.Server.Core.Dto;

public class CursorSearchRequestDto
{
    public const int MaxResponseSize = 10;

    [Required]
    public int NextCursor { get; set; }

    [Required]
    public string SearchQuery { get; set; } = string.Empty;
}
