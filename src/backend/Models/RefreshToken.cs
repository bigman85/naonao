using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HHPortal.Backend.Models
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; } = string.Empty;

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        [InverseProperty("RefreshTokens")]
        public User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}