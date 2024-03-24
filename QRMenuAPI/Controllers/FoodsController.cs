using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoodsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Foods
        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
        {
            if (_context.Foods == null)
            {
                return NotFound();
            }
            return await _context.Foods.ToListAsync();
        }

        // GET: api/Foods/5
        [HttpGet("getById{id}")]
        [Authorize]
        public async Task<ActionResult<Food>> GetFood(int id)
        {
            if (!await FoodExists(id))
            {
                return NotFound();
            }
            Food food = await _context.Foods.FindAsync(id);

            return food;
        }

        // PUT: api/Foods/5
        [HttpPut("update{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<ActionResult> PutFood(int id, Food food)
        {
            if (id != food.Id)
            {
                return BadRequest("Gönderilen ID, yiyeceğin ID'si ile uyuşmuyor.");
            }

            var category = await _context.Categories.FindAsync(food.CategoryId);
            if (category == null || !User.HasClaim("RestaurantId", category.RestaurantId.ToString()))
            {
                return Unauthorized("Bu işlem için yetkiniz yok veya kategori bulunamadı.");
            }

            _context.Entry(food).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await FoodExists(food.Id))
                {
                    return NotFound("Yiyecek bulunamadı.");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Bir eşzamanlılık hatası oluştu.");
                }
            }

            return Ok("Yiyecek başarıyla güncellendi.");
        }

        [HttpPost("add")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<ActionResult<int>> PostFood(Food food)
        {
            var category = await _context.Categories.FindAsync(food.CategoryId);
            if (category == null || !User.HasClaim("RestaurantId", category.RestaurantId.ToString()))
            {
                return Unauthorized("Bu işlem için yetkiniz yok veya kategori bulunamadı.");
            }

            try
            {
                await _context.Foods.AddAsync(food);
                await _context.SaveChangesAsync();
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Yiyecek eklenirken bir hata oluştu: {exc.Message}");
            }

            return Ok(food.Id);
        }



        // DELETE: api/Foods/5
        [HttpDelete("delete{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<ActionResult<int>> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(food.CategoryId);
            if (category == null || !User.HasClaim("RestaurantId", category.RestaurantId.ToString()))
            {
                return Unauthorized();
            }

            food.StateId = 0;
            _context.Foods.Update(food);

            var menus = _context.Menus.Where(m => m.CategoryId == category.Id);
            foreach (var menu in menus)
            {
                menu.StateId = 0;
                _context.Menus.Update(menu);
            }

            await _context.SaveChangesAsync();

            return Ok(food.Id);
        }

        private async Task<bool> FoodExists(int id)
        {
            return await _context.Foods.AnyAsync(f => f.Id == id);
        }
    }
}


















//// BENİM, REFACTORE EDİLMESİ GEREKEN KODLAR

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QRMenuAPI.Data;
//using QRMenuAPI.Models;

//namespace QRMenuAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FoodsController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public FoodsController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // GET: api/Foods
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
//        {
//            return await _context.Foods.ToListAsync();
//        }

//        // GET: api/Foods/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<Food>> GetFood(int id)
//        {
//            var food = await _context.Foods.FindAsync(id);

//            if (food == null)
//            {
//                return NotFound();
//            }

//            return food;
//        }

//        // PUT: api/Foods/5
//        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
//        [HttpPut("{id}")]
//        public async Task<IActionResult> PutFood(int id, Food food)
//        {
//            if (id != food.Id)
//            {
//                return BadRequest();
//            }

//            _context.Entry(food).State = EntityState.Modified;

//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!FoodExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }

//            return NoContent();
//        }

//        // POST: api/Foods
//        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
//        [HttpPost]
//        public async Task<ActionResult<Food>> PostFood(Food food)
//        {
//            _context.Foods.Add(food);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction("GetFood", new { id = food.Id }, food);
//        }

//        // DELETE: api/Foods/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteFood(int id)
//        {
//            var food = await _context.Foods.FindAsync(id);
//            if (food == null)
//            {
//                return NotFound();
//            }

//            _context.Foods.Remove(food);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        private bool FoodExists(int id)
//        {
//            return _context.Foods.Any(e => e.Id == id);
//        }
//    }
//}
