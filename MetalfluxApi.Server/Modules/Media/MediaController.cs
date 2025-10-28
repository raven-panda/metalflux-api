using MetalfluxApi.Server.Authentication.Service;
using MetalfluxApi.Server.Core.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetalfluxApi.Server.Modules.Media;

[Route("media")]
[ApiController]
public class MediaController(
    IMediaService service,
    TokenProvider tokenProvider,
    IConfiguration configuration
) : ControllerBase
{
    [HttpGet("user-selection")]
    [Authorize]
    public UserSelectionResponse GetUserSelection()
    {
        var (userId, _) = tokenProvider.ParseUserToken(
            Request.Cookies[configuration["Jwt:RefreshTokenCookieName"]!]!
        );

        var selection = service.GetUserSelection(userId);
        return selection;
    }

    [HttpPost("browse")]
    public CursorResponse<MediaDto> Search(CursorSearchRequestDto body)
    {
        var medias = service.Search(body, out var lastId, out var lastItemReached);
        return new CursorResponse<MediaDto>()
        {
            Data = medias,
            NextCursor = lastId,
            LastItemReached = lastItemReached,
        };
    }

    [HttpGet("{id:int}")]
    public MediaDto GetById(int id)
    {
        var media = service.Get(id);
        return media;
    }

    [HttpGet("{id:int}/stream/{chunk:int}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<FileStreamResult> GetMediaStream(int id, int chunk)
    {
        var (mediaStream, contentType, fileName) = await service.GetMediaStream(id, chunk);
        return File(mediaStream, contentType, fileName);
    }

    [HttpGet("{id:int}/stream-full")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<FileStreamResult> GetFullMediaStream(int id)
    {
        var (mediaStream, contentType, fileName) = await service.GetFullMediaStream(id);
        return File(mediaStream, contentType, fileName);
    }

    [Authorize]
    [HttpPost("create")]
    public MediaDto CreateMedia(MediaDto body)
    {
        var media = service.Add(body);
        return media;
    }

    [Authorize]
    [HttpPost("{id:int}/upload")]
    public async Task<MediaDto> UploadMedia([FromForm] int id, IFormFile file)
    {
        var media = await service.UploadMedia(id, file);
        return media;
    }
}
