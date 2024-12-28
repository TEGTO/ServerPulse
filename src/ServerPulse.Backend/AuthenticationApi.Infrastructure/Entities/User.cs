using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Infrastructure
{
    [Index(nameof(Email), IsUnique = true)]
    public class User : IdentityUser
    {
        [MaxLength(100)]
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }
        [Required]
        public DateTime RegisteredAtUtc { get; set; }
        [Required]
        public DateTime UpdatedAtUtc { get; set; }
    }
}
