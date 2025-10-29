namespace MetalfluxApi.Server.Core.Base;

public interface IRepositoryBase<TModel>
{
    TModel? Get(long? id);
    bool Exists(long? id);
    TModel Add(TModel item);
    long Remove(long id);
    TModel Update(TModel item);
}

public interface IAuditableRepositoryBase<TModel>
{
    TModel? Get(long? id);
    bool Exists(long? id);
    TModel Add(TModel item, long createdById);
    long Remove(long id);
    TModel Update(TModel item, long updatedById);
}
