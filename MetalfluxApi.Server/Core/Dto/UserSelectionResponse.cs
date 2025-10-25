using MetalfluxApi.Server.Modules.Media;

namespace MetalfluxApi.Server.Core.Dto;

public class UserSelectionResponse(List<MediaDto>? forYou, List<MediaDto>? popular)
{
    public List<MediaDto>? ForYou { get; set; } = forYou;
    public List<MediaDto>? Popular { get; set; } = popular;
}
