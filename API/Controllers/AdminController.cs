using API.Data;
using API.DTOs;
using API.Entity;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUnitOfWork uow;
        private readonly IPhotoService photoService;

        public AdminController(UserManager<AppUser> userManager, IUnitOfWork uow
            , IPhotoService photoService)
        {
            this.userManager = userManager;
            this.uow = uow;
            this.photoService = photoService;
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
        [HttpGet("get-photos-for-approval")]
        public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>>GetPhotosForApproval()
        {
            return Ok(await uow.PhotoRepository.GetUnapprovedPhotos());
        }

        [Authorize(Policy = "ModderatePhotoRole")]
        [HttpGet("approve-photo/{photoId}")]
        public async Task<ActionResult> AprovePhoto(int photoId)
        {
            var photo = await uow.PhotoRepository.GetPhotoById(photoId);

            var user = await uow.UserRepository.GetUserByUsernameAsync(photo.AppUser.UserName);

            if (photo == null) return NotFound();

            photo.IsApproved = true;

            if (user.Photos.Where(x => x.IsMain).Count() == 0) photo.IsMain = true ;

            if (await uow.Complete()) return Ok();

            return BadRequest("Failed to Approve photo");
        }

        [Authorize(Policy = "ModderatePhotoRole")]
        [HttpGet("reject-photo/{photoId}")]
        public async Task<ActionResult> rejectPhoto(int photoId)
        {
            var photo = await uow.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain == true) return BadRequest("You cannot delete the main photo");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
                uow.PhotoRepository.RemovePhoto(photo);
            }

            if (await uow.Complete()) return Ok();

            return BadRequest("Failed to reject photo");
        }
    }
}
