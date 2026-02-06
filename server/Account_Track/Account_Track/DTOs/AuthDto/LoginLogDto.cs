using Account_Track.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account_Track.DTOs.AuthDto
{
    public class LoginLogDto
    {
        public int LoginId { get; set; }
        public int UserId { get; set; }
        public required DateTime LoginAt { get; set; } = DateTime.UtcNow;
        public required string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}
