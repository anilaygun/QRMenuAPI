using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AppRolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogService _logService;

        public AppRolesController(AppDbContext context, RoleManager<AppRole> roleManager, ILogService logService)
        {
            _context = context;
            _roleManager = roleManager;
            _logService = logService;
        }

        // GET: api/AppRoles
        [HttpGet("getall")]
        [Authorize]
        public async Task<ActionResult<List<AppRole>>> GetAppRole()
        {
            List<AppRole> appRoles = await _roleManager.Roles.ToListAsync();
            if (!appRoles.Any())
            {
                await LogActionOutcome(false, "GetAllRoles", "No roles found");
                return NotFound();
            }

            await LogActionOutcome(true, "GetAllRoles", "Roles retrieved successfully");
            return appRoles;
        }

        [HttpGet("getbyid{id}")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<ActionResult<AppRole>> GetAppRole(string id)
        {
            AppRole appRole = await _roleManager.FindByIdAsync(id);
            if (appRole == null)
            {
                await LogActionOutcome(false, "GetRoleById", $"Role not found for ID: {id}");
                return NotFound();
            }

            await LogActionOutcome(true, "GetRoleById", $"Role retrieved for ID: {id}");
            return appRole;
        }

        [HttpPut("update{id}")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<IActionResult> PutAppRole(string id, AppRole appRole)
        {
            if (id != appRole.Id)
            {
                await LogActionOutcome(false, "UpdateRole", "Role ID mismatch");
                return BadRequest();
            }

            try
            {
                IdentityResult result = await _roleManager.UpdateAsync(appRole);
                if (!result.Succeeded)
                {
                    await LogActionOutcome(false, "UpdateRole", $"Failed to update role for ID: {id}");
                    return BadRequest(result.Errors);
                }

                await _context.SaveChangesAsync();
                await LogActionOutcome(true, "UpdateRole", $"Role updated for ID: {id}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AppRoleExists(id))
                {
                    await LogActionOutcome(false, "UpdateRole", $"Role not found for ID: {id}");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpPost("add")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<ActionResult<AppRole>> PostAppRole(AppRole appRole)
        {
            IdentityResult result = await _roleManager.CreateAsync(appRole);
            if (!result.Succeeded)
            {
                await LogActionOutcome(false, "AddRole", "Failed to create role");
                return BadRequest(result.Errors);
            }

            await LogActionOutcome(true, "AddRole", $"Role created with ID: {appRole.Id}");
            return CreatedAtAction("GetAppRole", new { id = appRole.Id }, appRole);
        }

        [HttpDelete("delete{id}")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<IActionResult> DeleteAppRole(string id)
        {
            AppRole appRole = await _roleManager.FindByIdAsync(id);
            if (appRole == null)
            {
                await LogActionOutcome(false, "DeleteRole", $"Role not found for ID: {id}");
                return NotFound();
            }

            IdentityResult result = await _roleManager.DeleteAsync(appRole);
            if (!result.Succeeded)
            {
                await LogActionOutcome(false, "DeleteRole", $"Failed to delete role for ID: {id}");
                return BadRequest(result.Errors);
            }

            await _context.SaveChangesAsync();
            await LogActionOutcome(true, "DeleteRole", $"Role deleted for ID: {id}");
            return NoContent();
        }

        private async Task<bool> AppRoleExists(string id)
        {
            return await _roleManager.FindByIdAsync(id) != null;
        }

        private async Task LogActionOutcome(bool success, string action, string message)
        {
            var logLevel = success ? "Information" : "Error";
            await _logService.LogAsync(new LogEntry { Message = $"{action}: {message}", Level = logLevel, Timestamp = DateTime.UtcNow });
        }
    }
}