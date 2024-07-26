using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Domain.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User : IdentityUser
    {
        [MaxLength(100)]
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
