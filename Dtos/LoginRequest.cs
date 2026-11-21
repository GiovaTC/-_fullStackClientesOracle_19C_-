using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public sealed class LoginRequest
    {
        [Required, StringLength(60, MinimumLength = 3)]
        public string Usuario { get; set; } = "";

        [Required, StringLength(128, MinimumLength = 12)]
        public string Password { get; set; } = "";  
    }
}
