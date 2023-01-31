using API.Extensions;

namespace API.Entity
{
    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }    
        public byte[] PasswordSalt { get; set; }   
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
        /*
        public int GetAge()
        {
            return DateOfBirth.CalcuateAge();
        }
        */
    }
}
