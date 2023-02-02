using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService , IMapper mapper)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _userManager = userManager;

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegistrerDto registrerDto)
        {
            if (UserExist(registrerDto.UserName)) return BadRequest("User Exsist");

            var user = _mapper.Map<AppUser>(registrerDto);

            user.UserName = registrerDto.UserName.ToLower();


            var result = await _userManager.CreateAsync(user , registrerDto.Password);

            if (!result.Succeeded) BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                token = await _tokenService.CreateToken(user),
                knownAs= user.KnownAs,
                Gender = user.Gender,
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);
            if (user == null) return Unauthorized("Invalid UserName");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized("Invalid userName");

            return new UserDto
            {
                Username = user.UserName,
                token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }

        private bool UserExist(string userName)
        {
            var userExist = _userManager.Users.Any(x => x.UserName == userName.ToLower());
            return userExist;
        }
        
    }

}
