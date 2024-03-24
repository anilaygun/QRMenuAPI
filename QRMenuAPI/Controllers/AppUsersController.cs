using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRMenuAPI.Data;
using QRMenuAPI.Models.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using QRMenuAPI.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using QRMenuAPI.Services;
using QRMenuAPI.Models;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUsersController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogService _logService;


        public AppUsersController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogService logService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
        }

        [HttpGet("getAll")]
        [Authorize]
        public ActionResult<List<AppUser>> GetAppUsers()
        {
            var users = _signInManager.UserManager.Users.ToList();
            if (!users.Any())
            {
                return NotFound();
            }

            return users;
        }

        [HttpGet("getById{id}")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<AppUser> GetAppUsers(string id)
        {
            AppUser appUser = _signInManager.UserManager.FindByIdAsync(id).Result;
            if (appUser == null)
            {
                return NotFound();
            }

            return appUser;
        }

        [HttpPut("update{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> PutAppUser(AppUser appUser)
        {
            AppUser existingAppUser = _signInManager.UserManager.FindByIdAsync(appUser.Id).Result;

            if (appUser == null)
            {
                return NotFound();
            }
            existingAppUser.Email = appUser.Email;
            existingAppUser.Name = appUser.Name;
            existingAppUser.PhoneNumber = appUser.PhoneNumber;
            existingAppUser.StateId = appUser.StateId;

            IdentityResult updateResult = await _signInManager.UserManager.UpdateAsync(existingAppUser);
            await LogActionOutcome(updateResult.Succeeded, appUser.Id, updateResult.Errors);

            return updateResult.Succeeded
                ? Ok(appUser.Id)
                : BadRequest(updateResult.Errors);
        }

        [HttpPost("addUser")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<IActionResult> PostAppUser(AppUser appUser, string password)
        {
            IdentityResult identityResult = await _signInManager.UserManager.CreateAsync(appUser, password);

            await LogActionOutcome(identityResult.Succeeded, appUser.Id, identityResult.Errors);

            if (identityResult.Succeeded)
            {
                return Ok(new { Result = identityResult, UserId = appUser.Id });
            }
            else
            {
                return BadRequest(identityResult.Errors);
            }
        }


        [HttpDelete("delete{id}")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<IActionResult> DeleteAppUser(string id)
        {
            AppUser appUser = await _signInManager.UserManager.FindByIdAsync(id);
            if (appUser == null)
            {
                return NotFound(new { Error = "Kullanıcı bulunamadı..." });
            }
            appUser.StateId = 0;
            IdentityResult identityResult = await _signInManager.UserManager.UpdateAsync(appUser);
            await LogActionOutcome(identityResult.Succeeded, id, identityResult.Errors);

            if (identityResult.Succeeded)
            {
                return Ok(new { Result = identityResult, UserId = appUser.Id });
            }
            else
            {
                return BadRequest(identityResult.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                await LogActionOutcome(false, userName, new IdentityError[] { new IdentityError { Description = "Giriş başarısız." } });
                return Unauthorized(new { Error = "Giriş başarısız." });
            }

            await LogActionOutcome(true, userName, null);
            return Ok(new { Message = "Giriş başarılı." });
        }



        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Başarıyla çıkış yapıldı." });
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDTO registerUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = new AppUser
            {
                UserName = registerUserDTO.UserName,
                Email = registerUserDTO.Email,
                PhoneNumber = "05119998877",
                StateId = 0,
                CompanyId = 1

            };

            IdentityResult userCreationResult = await _userManager.CreateAsync(newUser, registerUserDTO.Password);
            if (!userCreationResult.Succeeded)
            {
                foreach (var error in userCreationResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            // Kullanıcıya varsayılan rol atama
            var roleAssignResult = await _userManager.AddToRoleAsync(newUser, "DefaultRole");
            if (!roleAssignResult.Succeeded)
            {
                foreach (var error in roleAssignResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                await _userManager.DeleteAsync(newUser);
                return BadRequest(ModelState);
            }

            // Kullanıcıya claim atama
            var claimResult = await _userManager.AddClaimAsync(newUser, new Claim("DefaultClaim", registerUserDTO.UserName));
            if (!claimResult.Succeeded)
            {
                foreach (var error in claimResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                await _userManager.RemoveFromRoleAsync(newUser, "DefaultRole");
                await _userManager.DeleteAsync(newUser);
                return BadRequest(ModelState);
            }
            await LogActionOutcome(true, newUser.UserName, null);

            return Ok(new { message = "Kullanıcı başarıyla kaydedildi.", userName = newUser.UserName });
        }


        [HttpPost("passwordReset")]
        [Authorize]
        public async Task<IActionResult> PasswordReset(string userName)
        {
            var appUser = await _signInManager.UserManager.FindByNameAsync(userName);

            if (appUser == null)
            {
                await LogActionOutcome(false, userName, new IdentityError[] { new IdentityError { Description = "Kullanıcı bulunamadı." } });
                return NotFound(new { Error = "Kullanıcı bulunamadı." });
            }

            var resetToken = await _signInManager.UserManager.GeneratePasswordResetTokenAsync(appUser);
            await LogActionOutcome(true, userName);
            return Ok(new { Token = resetToken });
        }

        [HttpPost("validateToken")]
        [Authorize]
        public async Task<IActionResult> ValidateToken(string userName, string token, string newPassword)
        {
            var appUser = await _signInManager.UserManager.FindByNameAsync(userName);

            if (appUser == null)
            {
                await LogActionOutcome(false, userName, new IdentityError[] { new IdentityError { Description = "Kullanıcı bulunamadı." } });
                return NotFound(new { Error = "Kullanıcı bulunamadı." });
            }

            var identityResult = await _signInManager.UserManager.ResetPasswordAsync(appUser, token, newPassword);
            if (!identityResult.Succeeded)
            {
                await LogActionOutcome(false, userName, identityResult.Errors.ToArray());
                return BadRequest(identityResult.Errors.Select(e => e.Description));
            }

            await LogActionOutcome(true, userName);
            return Ok(new { Message = "Şifre başarıyla sıfırlandı." });
        }

        [HttpPost("assignRole")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignRole(string userId, string roleId)
        {
            AppUser appUser = await _userManager.FindByIdAsync(userId);
            AppRole appRole = await _roleManager.FindByIdAsync(roleId);

            if (appUser == null || appRole == null)
            {
                await LogActionOutcome(false, userId, new[] { new IdentityError { Description = "Kullanıcı veya rol bulunamadı." } });
                return NotFound(new { Error = "Kullanıcı veya rol bulunamadı." });
            }

            IdentityResult result = await _userManager.AddToRoleAsync(appUser, appRole.Name);
            if (result.Succeeded)
            {
                await LogActionOutcome(true, userId);
                return Ok(new { Message = "Rol başarıyla atandı." });
            }
            else
            {
                await LogActionOutcome(false, userId, result.Errors);
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("addClaim")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddClaim(string userName, string claimName)
        {
            AppUser appUser = await _signInManager.UserManager.FindByNameAsync(userName);

            if (appUser == null)
            {
                await LogActionOutcome(false, userName, new[] { new IdentityError { Description = "Kullanıcı bulunamadı." } });
                return NotFound(new { Error = "Kullanıcı bulunamadı." });
            }

            Claim claim = new Claim(claimName, appUser.Id);
            IdentityResult result = await _signInManager.UserManager.AddClaimAsync(appUser, claim);

            if (result.Succeeded)
            {
                await LogActionOutcome(true, userName);
                return Ok(new { Message = "Claim başarıyla eklendi." });
            }
            else
            {
                await LogActionOutcome(false, userName, result.Errors);
                return BadRequest(result.Errors);
            }
        }

        private bool ApplicationUserExists(string id)
        {
            return (_userManager.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        //////////////////////////////////

        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginDTO loginDTO)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        AppUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
        //        if (user != null)
        //        {
        //            await _signInManager.SignOutAsync();
        //            Microsoft.AspNetCore.Identity.SignInResult result =
        //                await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.Persistent, loginDTO.Lock);

        //            if (result.Succeeded)
        //            {
        //                Response.Cookies.Append("AspNetCoreIdentityCookie", "cookie-value");
        //                return Ok(new { message = "Giriş başarılı." });
        //            }
        //            else
        //            {
        //                return Unauthorized(new { message = "E-posta veya şifre yanlış." });
        //            }
        //        }
        //        else
        //        {
        //            return NotFound(new { message = "Böyle bir kullanıcı bulunmamaktadır." });
        //        }
        //    }
        //    return BadRequest(ModelState);
        //}

        // Log metodu;
        private async Task LogActionOutcome(bool success, string userId, IEnumerable<IdentityError> errors = null)
        {
            var message = success ? $"User updated: {userId}" : $"Update failed for user {userId}: {string.Join(", ", errors.Select(e => e.Description))}";
            var level = success ? "Information" : "Error";
            await _logService.LogAsync(new LogEntry { Message = message, Level = level, Timestamp = DateTime.UtcNow });
        }

    }

}



