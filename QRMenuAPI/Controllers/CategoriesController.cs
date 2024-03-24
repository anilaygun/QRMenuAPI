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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet("getAll")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }

            return await _context.Categories.ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("getById{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            if (!CategoryExists(id))
            {
                return NotFound();
            }
            Category category = await _context.Categories.FindAsync(id);

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("update{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            id = category.RestaurantId;
            if (User.HasClaim("RestaurantId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Categories
        [HttpPost("add")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            if (!CategoryExists(category.Id))
            {
                return NotFound();
            }
            if (User.HasClaim("RestauranId", category.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // kontrol

            Category category = await _context.Categories.FindAsync(id);
            if (!CategoryExists(id))
            {
                return BadRequest();
            }
            if (User.HasClaim("RestauranId", category.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }

            try
            {
                category.StateId = 0;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return Ok();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
