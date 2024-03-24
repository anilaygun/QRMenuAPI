using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/States
        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<State>>> GetStates()
        {
            return await _context.States.ToListAsync();
        }

        // GET: api/States/5
        [HttpGet("getById{id}")]
        public async Task<ActionResult<State>> GetState(byte id)
        {
            State state = await _context.States.FindAsync(id);

            if (state == null)
            {
                return NotFound();
            }

            return state;
        }
    }
}
