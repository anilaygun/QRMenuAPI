using Microsoft.AspNetCore.Identity;

namespace QRMenuAPI.CustomValidations
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateUserName(string userName) => new IdentityError { Code = "DuplicateUserName", Description = $"\"{userName}\" kullanıcı adı kullanılmaktadır." };
        public override IdentityError InvalidUserName(string userName) => new IdentityError { Code = "InvalidUserName", Description = "Geçersiz kullanıcı adı." };
        public override IdentityError DuplicateEmail(string email) => new IdentityError { Code = "DuplicateEmail", Description = $"\"{email}\" başka bir kullanıcı tarafından kullanılmaktadır." };
        public override IdentityError InvalidEmail(string email) => new IdentityError { Code = "InvalidEmail", Description = "Geçersiz email." };
        public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = "PasswordTooShort", Description = $"Şifre çok kısa. En az {length} karakter olmalıdır." };
        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = "PasswordRequiresNonAlphanumeric", Description = "Şifre en az bir alfasayısal olmayan karakter içermelidir." };
        public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = "PasswordRequiresDigit", Description = "Şifre en az bir rakam içermelidir." };
        public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = "PasswordRequiresUpper", Description = "Şifre en az bir büyük harf içermelidir." };
        public override IdentityError PasswordRequiresLower() => new IdentityError { Code = "PasswordRequiresLower", Description = "Şifre en az bir küçük harf içermelidir." };
    }
}