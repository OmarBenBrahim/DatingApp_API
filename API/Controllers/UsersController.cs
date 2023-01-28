using API.Data;
using API.DTOs;
using API.Entity;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    //[Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRpository userRpository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRpository userRpository, IMapper mapper, IPhotoService photoService)
        {
            this.userRpository = userRpository;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<PageList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUser = await userRpository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = currentUser.UserName;
            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await userRpository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(
                new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages)
                );
            return Ok(users);

        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await userRpository.GetMemberAsync(username);
           
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await userRpository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            mapper.Map(memberUpdateDto, user);
            if(await userRpository.SaveAllAsync()) return NoContent();

            return BadRequest("Faild to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> addPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await userRpository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            var result = await photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if(user.Photos.Count == 0) photo.IsMain= true;

            user.Photos.Add(photo);

            if (await userRpository.SaveAllAsync())
            {
                //return mapper.Map<PhotoDto>(photo);
                return CreatedAtAction(nameof(GetUser),
                    new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult<PhotoDto>> SetMain(int photoId)
        {
            var user = await userRpository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);

            if(photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("this is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if(await userRpository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRpository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if(await userRpository.SaveAllAsync() ) return NoContent();

            return BadRequest("Problem Deleting Photo ");

        }

    }
}
