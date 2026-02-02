namespace Account_Track.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public required T Data { get; set; }
        public PaginationDto? Pagination { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required string TraceId { get; set; }
    }
}
