using JwtAuthAspNet7WebAPI.Core.Dtos;

namespace JwtAuthAspNet7WebAPI.Interface
{
    public interface IUserRepository
    {
        ICollection<UserDto> getUsers();
    }
}
