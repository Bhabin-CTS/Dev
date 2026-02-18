namespace Account_Track.DTOs.AuthDto
{
    public class RefreshTokenRequestDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
