using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        public AccountController(DataContext context, ITokenService tokenService , IMapper mapper)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _context = context;
            

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegistrerDto registrerDto)
        {
            if (UserExist(registrerDto.UserName)) return BadRequest("User Exsist");

            var user = _mapper.Map<AppUser>(registrerDto);

            using var hmac = new HMACSHA512();

            user.UserName = registrerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registrerDto.Password));
            user.PasswordSalt = hmac.Key;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                token = _tokenService.CreateToken(user),
                knownAs= user.KnownAs,
                Gender = user.Gender,
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);
            if (user == null) return Unauthorized("Invalid UserName");
            
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0; i<computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password"); 
            }

            return new UserDto
            {
                Username = user.UserName,
                token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownAs = user.KnownAs,
                Gender = user.Gender,
            };
        }

        private bool UserExist(string userName)
        {
            var userExist = _context.Users.Any(x => x.UserName == userName.ToLower());
            return userExist;
        }
        
    }

}
