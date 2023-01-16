using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegistrerDto
    {
        public string UserName { get; set; }

        [StringLength(8, MinimumLength = 4)]
        public string Password { get; set; }
    }
}
