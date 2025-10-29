namespace MetalfluxApi.Server.Core.Base;

public interface IService<TDto, TModel>
{
    TDto Get(long id);
    TDto Add(TDto item);
    long Remove(long id);
    TDto Update(TDto item);
    TDto ToDto(TModel model);
    public TModel ToModel(TDto dto);
}

public interface IAuditableService<TDto, TModel, in TCreateDto, in TUpdateDto>
{
    TDto Get(long id);
    TDto Add(TCreateDto item, long createdById);
    long Remove(long id);
    TDto Update(TUpdateDto item, long updatedById);
    TDto ToDto(TModel model);
    public TModel ToModelForCreation(TCreateDto dto);
    public TModel ToModelForUpdate(TUpdateDto dto);
}
