using MetalfluxApi.Server.Core.Base;
using MetalfluxApi.Server.Core.Dto;

namespace MetalfluxApi.Server.Modules.Media;

public interface IMediaRepository : IRepositoryBase<MediaModel>
{
    List<MediaModel> Search(CursorSearchRequestDto body, out int lastId, out bool lastItemReached);
}

internal sealed class MediaRepository(AppDbContext context) : IMediaRepository
{
    public MediaModel? Get(int? id)
    {
        return context.Medias.Find(id);
    }

    public bool Exists(int? id)
    {
        return context.Medias.Any(item => item.Id == id);
    }

    public MediaModel Add(MediaModel item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.HasUploadedMedia = false;

        context.Medias.Add(item);
        context.SaveChanges();

        return item;
    }

    public int Remove(int id)
    {
        var entity = context.Medias.Find(id)!;
        context.Medias.Remove(entity);
        context.SaveChanges();
        return entity.Id;
    }

    public MediaModel Update(MediaModel item)
    {
        var entity = context.Medias.Find(item.Id)!;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.Name = item.Name;

        context.Medias.Update(entity);
        context.SaveChanges();
        return entity;
    }

    public List<MediaModel> Search(
        CursorSearchRequestDto body,
        out int lastId,
        out bool lastItemReached
    )
    {
        var query = context
            .Medias.Where(m => m.Name.Contains(body.SearchQuery))
            .Where(m => m.HasUploadedMedia)
            .Where(m => m.Id > body.NextCursor)
            .OrderBy(m => m.Id)
            .Take(CursorSearchRequestDto.MaxResponseSize);
        var results = query.ToList();

        lastId = results.Count > 0 ? results.Last().Id : body.NextCursor;
        lastItemReached = CursorSearchRequestDto.MaxResponseSize >= results.Count;

        return results;
    }
}
