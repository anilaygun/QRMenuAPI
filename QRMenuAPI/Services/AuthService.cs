//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.IdentityModel.Tokens;
//using QRMenuAPI.Models.Authentication;
//using QRMenuAPI.Models.DTOs;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;

//namespace QRMenuAPI.Services
//{
//    public class AuthService : IAuthService
//    {
//        private readonly UserManager<AppUser> _userManager;
//        private readonly IConfiguration _config;

//        public AuthService(UserManager<AppUser> userManager, IConfiguration config)
//        {
//            _userManager = userManager;
//            _config = config;
//        }

//        public async Task<bool> RegisterUser(LoginUser user)
//        {
//            var identityUser = new AppUser
//            {
//                UserName = user.UserName,
//                Email = user.UserName,
//                PhoneNumber="111111111",
//                StateId=0,
//                CompanyId=1

//            };

//            IdentityResult result =  _userManager.CreateAsync(identityUser, user.Password).Result;
//          //  return result.Succeeded;
//        if(result.Succeeded)
//                return true;
//        return false;
//        }

//        public async Task<LoginResponse> Login(LoginUser user)
//        {
//            var response = new LoginResponse();
//            var identityUser = await _userManager.FindByEmailAsync(user.UserName);

//            if (identityUser is null || (await _userManager.CheckPasswordAsync(identityUser, user.Password)) == false)
//            {
//                return response;
//            }

//            response.IsLogedIn = true;
//            response.JwtToken = this.GenerateTokenString(identityUser.Email);
//            response.RefreshToken = this.GenerateRefreshTokenString();

//            identityUser.RefreshToken = response.RefreshToken;
//            identityUser.RefreshTokenExpiry = DateTime.Now.AddHours(12);
//            await _userManager.UpdateAsync(identityUser);

//            return response;
//        }

//        public async Task<LoginResponse> RefreshToken(RefreshTokenModel model)
//        {
//            var principal = GetTokenPrincipal(model.JwtToken);

//            var response = new LoginResponse();
//            if (principal?.Identity?.Name is null)
//                return response;

//            var identityUser = await _userManager.FindByNameAsync(principal.Identity.Name);

//            if (identityUser is null || identityUser.RefreshToken != model.RefreshToken || identityUser.RefreshTokenExpiry < DateTime.Now)
//                return response;

//            response.IsLogedIn = true;
//            response.JwtToken = this.GenerateTokenString(identityUser.Email);
//            response.RefreshToken = this.GenerateRefreshTokenString();

//            identityUser.RefreshToken = response.RefreshToken;
//            identityUser.RefreshTokenExpiry = DateTime.Now.AddHours(12);
//            await _userManager.UpdateAsync(identityUser);

//            return response;
//        }

//        private ClaimsPrincipal? GetTokenPrincipal(string token)
//        {

//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

//            var validation = new TokenValidationParameters
//            {
//                IssuerSigningKey = securityKey,
//                ValidateLifetime = false,
//                ValidateActor = false,
//                ValidateIssuer = false,
//                ValidateAudience = false,
//            };
//            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
//        }

//        private string GenerateRefreshTokenString()
//        {
//            var randomNumber = new byte[64];

//            using (var numberGenerator = RandomNumberGenerator.Create())
//            {
//                numberGenerator.GetBytes(randomNumber);
//            }

//            return Convert.ToBase64String(randomNumber);
//        }

//        private string GenerateTokenString(string userName)
//        {
//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name,userName),
//                new Claim(ClaimTypes.Role,"Administrator"),
//            };

//            var staticKey = _config.GetSection("Jwt:Key").Value;
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(staticKey));
//            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

//            var securityToken = new JwtSecurityToken(
//                claims: claims,
//                expires: DateTime.Now.AddSeconds(20),
//                signingCredentials: signingCred
//                );

//            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
//            return tokenString;
//        }
//    }
//}
