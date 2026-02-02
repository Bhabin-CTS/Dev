namespace Account_Track.DTOs
{
    public class ErrorResponseDto
    {
        public bool Success { get; set; } = false;
        public required string ErrorCode { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required string TraceId { get; set; }
    }
}
