namespace MetalfluxApi.Server.Core.Dto;

public struct CursorResponse<TDto>
{
    public List<TDto> Data { get; set; }
    public long NextCursor { get; set; }
    public bool LastItemReached { get; set; }
}
