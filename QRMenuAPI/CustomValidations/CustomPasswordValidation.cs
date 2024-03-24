using Microsoft.AspNetCore.Identity;
using QRMenuAPI.Models.Authentication;

namespace QRMenuAPI.CustomValidations
{
    public class CustomPasswordValidation : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (password.Length < 5 || password.Length > 25)
            {
                errors.Add(new IdentityError { Code = "PasswordLength", Description = "Şifreniz 5 - 25 karakter arasında olmalıdır..." });
            }
            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                errors.Add(new IdentityError { Code = "PasswordContainsUserName", Description = "Lütfen şifre içerisinde kullanıcı adını yazmayınız." });
            }
            if (!password.Any(char.IsDigit))
            {
                errors.Add(new IdentityError { Code = "PasswordRequiresDigit", Description = "Şifreniz en az bir rakam içermelidir." });
            }
            if (!password.Any(char.IsUpper))
            {
                errors.Add(new IdentityError { Code = "PasswordRequiresUpper", Description = "Şifreniz en az bir büyük harf içermelidir." });
            }
            if (!password.Any(char.IsLower))
            {
                errors.Add(new IdentityError { Code = "PasswordRequiresLower", Description = "Şifreniz en az bir küçük harf içermelidir." });
            }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errors.Add(new IdentityError { Code = "PasswordRequiresSpecial", Description = "Şifreniz en az bir özel karakter içermelidir. Örneğin: @, #, $" });
            }
            if (password.Intersect("0123456789").Count() < 2)
            {
                errors.Add(new IdentityError { Code = "PasswordRequiresMultipleDigits", Description = "Şifreniz en az iki rakam içermelidir." });
            }
            if (password.Intersect("!@#$%^&*()_+-=[]{}|;:'\",.<>?/`~").Count() < 2)
            {
                errors.Add(new IdentityError { Code = "PasswordRequiresMultipleSpecial", Description = "Şifreniz en az iki özel karakter içermelidir." });
            }

            return errors.Any() ? Task.FromResult(IdentityResult.Failed(errors.ToArray())) : Task.FromResult(IdentityResult.Success);
        }
    }
}
