using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class UserDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public string userId { get; set; }
    }
}
