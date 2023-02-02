using API.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                }).ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditeRoles(string username, [FromQuery] string roles )
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("select at least one role");

            var selectdRoles = roles.Split(',').ToArray();
            var user = await userManager.FindByNameAsync(username);

            if(user == null) return NotFound();

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectdRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Faild to add roles");

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectdRoles));

            if (!result.Succeeded) return BadRequest("Faild to Remove roles");

            return Ok(await userManager.GetRolesAsync(user));

        }


        [Authorize(Policy = "ModderatePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("admins Or moderators can see this");
        }
    }
}
