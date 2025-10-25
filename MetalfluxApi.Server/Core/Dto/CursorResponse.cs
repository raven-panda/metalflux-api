namespace MetalfluxApi.Server.Core.Dto;

public struct CursorResponse<TDto>
{
    public List<TDto> Data { get; set; }
    public int NextCursor { get; set; }
    public bool LastItemReached { get; set; }
}
