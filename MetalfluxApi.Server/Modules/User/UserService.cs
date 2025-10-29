using MetalfluxApi.Server.Core.Base;
using MetalfluxApi.Server.Core.Exceptions;

namespace MetalfluxApi.Server.Modules.User;

public interface IUserService : IService<UserDto, UserModel>
{
    public UserDto? TryGetByEmail(string email);
}

internal sealed class UserService(IUserRepository repo) : IUserService
{
    public UserDto Get(long id)
    {
        var item = repo.Get(id);
        if (item == null)
            throw new EntityNotFoundException("User", id);

        return ToDto(item);
    }

    public UserDto? TryGetByEmail(string email)
    {
        var item = repo.GetByEmail(email);

        return item != null ? ToDto(item) : null;
    }

    public UserDto Add(UserDto item)
    {
        throw new NotImplementedException();
    }

    public long Remove(long id)
    {
        if (!repo.Exists(id))
            throw new EntityNotFoundException("User", id);

        return repo.Remove(id);
    }

    public UserDto Update(UserDto item)
    {
        throw new AccessViolationException("Cannot update user");
    }

    public UserDto ToDto(UserModel model)
    {
        return new UserDto
        {
            Id = model.Id,
            Username = model.Username,
            Email = model.Email,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
        };
    }

    public UserModel ToModel(UserDto dto)
    {
        var userModel = repo.Get(dto.Id);

        return userModel
            ?? new UserModel
            {
                Username = dto.Username,
                Email = dto.Email,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
            };
    }
}
