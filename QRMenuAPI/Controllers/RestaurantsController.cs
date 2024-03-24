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
    public class RestaurantsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogService _logService;
        private readonly UserManager<AppUser> _userManager;

        public RestaurantsController(AppDbContext context, ILogService logService, UserManager<AppUser> userManager)
        {
            _context = context;
            _logService = logService;
            _userManager = userManager;
        }

        // GET: api/Restaurants
        [HttpGet("getAll")]
        [Authorize(Roles = "CompanyAdministrator,RestaurantAdministrator")]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            return await _context.Restaurants.ToListAsync();
        }

  
        // GET: api/Restaurants/5
        [HttpGet("getById{id}")]
        [Authorize(Roles = "CompanyAdministrator,RestaurantAdministrator")]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            Restaurant restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return restaurant;
        }

        // PUT: api/Restaurants/5
        [HttpPut("update{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<IActionResult> PutRestaurant(int id, Restaurant restaurant)
        {
            if (!User.HasClaim("RestaurantId", id.ToString()))
            {
                await LogActionOutcome(false, "Yanlış kullanıcı.");
                return Content("Yanlış kullanıcı.");
            }

            _context.Entry(restaurant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await LogActionOutcome(true, "Restoran başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                await LogActionOutcome(false, $"Güncelleme sırasında hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Güncelleme sırasında bir hata oluştu.");
            }

            return Ok("Restoran başarıyla güncellendi.");
        }

        // POST: api/Restaurants
        [HttpPost("add")]
        [Authorize(Roles = "CompanyAdministrator")]
        public async Task<ActionResult<int>> PostRestaurant(Restaurant restaurant)
        {
            AppUser appUser = new AppUser
            {
                CompanyId = restaurant.CompanyId,
                Email = restaurant.Email,
                Name = restaurant.Name,
                PhoneNumber = restaurant.Phone,
                RegisterDate = DateTime.Today,
                StateId = restaurant.StateId,
                UserName = restaurant.Name + "User"
            };

            Claim claim = new Claim("RestaurantId", restaurant.Id.ToString());

            try
            {
                await _context.Restaurants.AddAsync(restaurant);
                await _context.SaveChangesAsync();

                IdentityResult result = await _userManager.CreateAsync(appUser, "Admin123!");
                if (!result.Succeeded)
                {
                    await LogActionOutcome(false, "Kullanıcı oluşturulamadı.");
                    return BadRequest(result.Errors);
                }

                await _userManager.AddClaimAsync(appUser, claim);
                await _userManager.AddToRoleAsync(appUser, "RestaurantAdministrator");
                await LogActionOutcome(true, "Restoran ve kullanıcı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                await LogActionOutcome(false, $"Restoran oluşturma sırasında hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Restoran oluşturma sırasında bir hata oluştu.");
            }

            return Ok(restaurant.Id);
        }

        // DELETE: api/Restaurants/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdministrator,RestaurantAdministrator")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            if (!await RestaurantExistsAsync(id))
            {
                await LogActionOutcome(false, "Restoran bulunamadı.");
                return NotFound("Restoran bulunamadı.");
            }

            if (!User.HasClaim("RestaurantId", id.ToString()))
            {
                await LogActionOutcome(false, "Bu işlem için yetkiniz yok.");
                return Unauthorized("Bu işlem için yetkiniz yok.");
            }

            try
            {
                var restaurant = await _context.Restaurants.FindAsync(id);
                await SoftDeleteRestaurantAndRelatedEntities(restaurant);

                await _context.SaveChangesAsync();
                await LogActionOutcome(true, "Restoran ve ilişkili kayıtlar başarıyla pasifleştirildi.");
            }
            catch (Exception ex)
            {
                await LogActionOutcome(false, $"Hata: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "İşlem sırasında bir hata oluştu.");
            }

            return Ok("Restoran ve ilişkili kayıtlar başarıyla pasifleştirildi.");
        }

        private async Task<bool> RestaurantExistsAsync(int id)
        {
            return await _context.Restaurants.AnyAsync(e => e.Id == id);
        }

        private async Task SoftDeleteRestaurantAndRelatedEntities(Restaurant restaurant)
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

            // Restoranın kullanıcısını pasifleştirme işlemi
            var user = await _context.RestaurantUsers.FirstOrDefaultAsync(u => u.RestaurantId == restaurant.Id);
            if (user != null)
            {
                user.AppUser.StateId = 0;
                _context.Users.Update(user.AppUser);
            }
        }

        private async Task LogActionOutcome(bool success, string message)
        {
            var logLevel = success ? "Information" : "Error";
            await _logService.LogAsync(new LogEntry { Message = message, Level = logLevel, Timestamp = DateTime.UtcNow });
        }


    }
}