using System;

namespace Account_Track.DTOs
{
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }
        public int UserId { get; set; }
        public int LoginId { get; set; }
        public string EntityType { get; set; } = default!;
        public int EntityId { get; set; }
        public string Action { get; set; } = default!;
        public string? BeforeState { get; set; }
        public string AfterState { get; set; } = default!;
        public DateTime CreatedAt { get; set; } // UTC

        // Added fields
        public string? ChangedByName { get; set; }
        public string? ChangedByRole { get; set; }
    }
}