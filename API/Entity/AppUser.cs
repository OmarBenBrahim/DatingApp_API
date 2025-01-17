﻿using API.Extensions;
using Microsoft.AspNetCore.Identity;

namespace API.Entity
{
    public class AppUser : IdentityUser<int>
    {
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? KnownAs { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastActive { get; set; }
        public string? Introduction { get; set; }
        public string? LookingFor { get; set; }
        public string? Interests { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public List<Photo> Photos { get; set; } = new();
        public List<UserLike> likedByUsers { get; set; }
        public List<UserLike> LikedUsers { get; set; }
        public List<Message> MessagesSent { get; set; }
        public List<Message> MessagesReceived { get; set; }
        public ICollection<AppUserRole> UserRoles { get; set; }
        /*
        public int GetAge()
        {
            return DateOfBirth.CalcuateAge();
        }
        */
    }
}
