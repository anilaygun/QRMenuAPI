using Microsoft.AspNetCore.Identity;
using QRMenuAPI.Models.Authentication;


namespace QRMenuAPI.CustomValidations
{
    public class CustomUserValidation : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (int.TryParse(user.UserName[0].ToString(), out int _))
            {
                errors.Add(new IdentityError { Code = "UserNameNumberStartWith", Description = "Kullanıcı adı sayısal ifadeyle başlayamaz..." });
            }
            if (user.UserName.Length < 3 || user.UserName.Length > 15)
            {
                errors.Add(new IdentityError { Code = "UserNameLength", Description = "Kullanıcı adı 3 - 15 karakter arasında olmalıdır..." });
            }
            if (user.Email.Length > 70)
            {
                errors.Add(new IdentityError { Code = "EmailLength", Description = "Email 70 karakterden fazla olamaz..." });
            }
            if (!user.Email.Contains("@") || !user.Email.Contains("."))
            {
                errors.Add(new IdentityError { Code = "EmailFormat", Description = "Geçersiz email formatı..." });
            }
            if (manager.PasswordHasher.HashPassword(user, user.PasswordHash).Length < 6)
            {
                errors.Add(new IdentityError { Code = "PasswordSecurity", Description = "Şifre çok zayıf..." });
            }
            if (user.Email.EndsWith(".com") && user.Email.IndexOf("@") > user.Email.LastIndexOf(".com"))
            {
                errors.Add(new IdentityError { Code = "EmailInvalidPlacement", Description = "Email adresindeki '@' karakteri '.com' uzantısından önce olmalıdır..." });
            }
            if (!user.Email.ToLower().EndsWith(".com") && !user.Email.ToLower().EndsWith(".net") && !user.Email.ToLower().EndsWith(".org"))
            {
                errors.Add(new IdentityError { Code = "EmailDomain", Description = "Email yalnızca '.com', '.net' veya '.org' ile bitmelidir..." });
            }
            if (user.UserName.Intersect("!#$%&'*+/=?^_`{|}~").Any())
            {
                errors.Add(new IdentityError { Code = "UserNameSpecialChars", Description = "Kullanıcı adında geçersiz özel karakterler bulunmaktadır..." });
            }

            return errors.Any() ? Task.FromResult(IdentityResult.Failed(errors.ToArray())) : Task.FromResult(IdentityResult.Success);
        }
    }
}
