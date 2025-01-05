using System.ComponentModel.DataAnnotations;

namespace Task.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
