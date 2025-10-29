using MetalfluxApi.Server.Core.Base;

namespace MetalfluxApi.Server.Modules.User;

public interface IUserRepository : IRepositoryBase<UserModel>
{
    bool ExistsByEmail(string email);
    UserModel? GetByEmail(string email);
}

internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public UserModel? Get(long? id)
    {
        return context.Users.Find(id);
    }

    public UserModel? GetByEmail(string email)
    {
        return context.Users.FirstOrDefault(user => user.Email == email);
    }

    public bool Exists(long? id)
    {
        return context.Users.Any(item => item.Id == id);
    }

    public bool ExistsByEmail(string email)
    {
        return context.Users.Any(item => item.Email == email);
    }

    public UserModel Add(UserModel item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        context.Users.Add(item);
        context.SaveChanges();
        return item;
    }

    public long Remove(long id)
    {
        var entity = context.Users.Find(id)!;
        context.Users.Remove(entity);
        context.SaveChanges();
        return entity.Id;
    }

    public UserModel Update(UserModel item)
    {
        throw new AccessViolationException("Cannot update user");
    }
}
