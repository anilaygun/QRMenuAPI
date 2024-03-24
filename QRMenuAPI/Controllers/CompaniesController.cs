using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;
using QRMenuAPI.Models.Authentication;
using QRMenuAPI.Services;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogService _logService;
        private readonly UserManager<AppUser> _userManager;

        public CompaniesController(AppDbContext context, SignInManager<AppUser> signInManager, ILogService logService, UserManager<AppUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _logService = logService;
            _userManager = userManager;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            if (_context.Companies == null)
            {
                return NotFound();
            }
            return await _context.Companies.ToListAsync();
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            if (_context.Companies == null)
            {
                return NotFound();
            }
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public async Task<IActionResult> PutCompany(int id, Company company)
        {
            if (id != company.Id)
            {
                await LogActionOutcome(false, "Şirket ID'si uyuşmuyor.");
                return BadRequest("Şirket ID'si uyuşmuyor.");
            }
            if (!User.HasClaim("CompanyId", id.ToString()))
            {
                await LogActionOutcome(false, "Bu işlem için yetkiniz yok.");
                return Unauthorized("Bu işlem için yetkiniz yok.");
            }

            _context.Entry(company).State = EntityState.Modified;

            if (!await CompanyExistsAsync(id))
            {
                await LogActionOutcome(false, "Şirket bulunamadı.");
                return NotFound("Şirket bulunamadı.");
            }

            await _context.SaveChangesAsync();
            await LogActionOutcome(true, $"Şirket başarıyla güncellendi: {id}");

            return NoContent();
        }


        [HttpPost("add")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PostCompany(Company company)
        {
            if (_context.Companies == null)
            {
                await LogActionOutcome(false, "Şirket eklenemedi: Entity set 'AppDbContext.Companies' null.");
                return Problem("Entity set 'AppDbContext.Companies' null.");
            }

            AppUser applicationUser = new AppUser
            {
                CompanyId = company.Id,
                Email = company.Email,
                Name = company.Name,
                PhoneNumber = company.Phone,
                RegisterDate = DateTime.Today,
                StateId = 1,
                UserName = company.Name + company.Id.ToString()
            };

            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();

            IdentityResult userResult = await _signInManager.UserManager.CreateAsync(applicationUser, "Admin123!");
            if (!userResult.Succeeded)
            {
                await LogActionOutcome(false, "Kullanıcı oluşturulamadı: " + string.Join(", ", userResult.Errors.Select(e => e.Description)));
                return BadRequest(userResult.Errors);
            }

            Claim claim = new Claim("CompanyId", company.Id.ToString());
            IdentityResult claimResult = await _signInManager.UserManager.AddClaimAsync(applicationUser, claim);
            if (!claimResult.Succeeded)
            {
                await LogActionOutcome(false, "Claim eklenemedi: " + string.Join(", ", claimResult.Errors.Select(e => e.Description)));
                return BadRequest(claimResult.Errors);
            }

            IdentityResult roleResult = await _signInManager.UserManager.AddToRoleAsync(applicationUser, "CompanyAdministrator");
            if (!roleResult.Succeeded)
            {
                await LogActionOutcome(false, "Rol atanamadı: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return BadRequest(roleResult.Errors);
            }

            await LogActionOutcome(true, $"{company.Name}, Şirketi ve {applicationUser.Name}, Kullanıcısı başarıyla eklenmiştir!");
            return Ok($"{company.Name}, Şirketi ve {applicationUser.Name}, Kullanıcısı eklenmiştir!");
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (!await CompanyExistsAsync(id))
            {
                await LogActionOutcome(false, "Şirket bulunamadı.");
                return NotFound("Şirket bulunamadı.");
            }

            if (!User.HasClaim("CompanyId", id.ToString()) && !User.IsInRole("Administrator"))
            {
                await LogActionOutcome(false, "Bu işlem için yetkiniz yok.");
                return Unauthorized("Bu işlem için yetkiniz yok.");
            }

            try
            {
                Company company = await _context.Companies.FindAsync(id);
                await SoftDeleteCompanyAndRelatedEntities(company);

                await _context.SaveChangesAsync();
                await LogActionOutcome(true, "Şirket ve ilişkili kayıtlar başarıyla pasifleştirildi.");
            }
            catch (Exception ex)
            {
                await LogActionOutcome(false, $"Hata: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "İşlem sırasında bir hata oluştu.");
            }

            return Ok("Şirket ve ilişkili kayıtlar başarıyla pasifleştirildi.");
        }

        private async Task<bool> CompanyExistsAsync(int id)
        {
            return await _context.Companies.AnyAsync(e => e.Id == id);
        }


        private async Task LogActionOutcome(bool success, string message)
        {
            var logLevel = success ? "Information" : "Error";
            await _logService.LogAsync(new LogEntry { Message = message, Level = logLevel, Timestamp = DateTime.UtcNow });
        }

        private async Task SoftDeleteCompanyAndRelatedEntities(Company company)
        {
            company.StateId = 0;
            _context.Companies.Update(company);

            var restaurants = _context.Restaurants.Where(r => r.CompanyId == company.Id);
            foreach (Restaurant restaurant in restaurants)
            {
                restaurant.StateId = 0;
                _context.Restaurants.Update(restaurant);

                var categories = _context.Categories.Where(c => c.RestaurantId == restaurant.Id);
                foreach (Category category in categories)
                {
                    category.StateId = 0;
                    _context.Categories.Update(category);

                    var foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                    foreach (Food food in foods)
                    {
                        food.StateId = 0;
                        _context.Foods.Update(food);
                    }

                    var menus = _context.Menus.Where(m => m.CategoryId == category.Id);
                    foreach (Menu menu in menus)
                    {
                        menu.StateId = 0;
                        _context.Menus.Update(menu);
                    }
                }
            }

            var users = _userManager.Users.Where(u => u.CompanyId == company.Id);
            foreach (AppUser user in users)
            {
                user.StateId = 0;
                _context.Users.Update(user);
            }
        }
    }
}