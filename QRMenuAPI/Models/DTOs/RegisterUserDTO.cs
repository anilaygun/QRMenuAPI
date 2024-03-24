using System.ComponentModel.DataAnnotations;

namespace QRMenuAPI.Models.DTOs
{
    public class RegisterUserDTO
    {
        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = "";
    }
}
