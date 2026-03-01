using System;

namespace Account_Track.DTOs
{

    public class AuditLogQueryDto
    {
        // Filters (nullable)
        public int? UserId { get; set; }
        public int? LoginId { get; set; }
        public string? EntityType { get; set; }   // e.g., "Invoice", "Account", etc.
        public int? EntityId { get; set; }

        public string? Action { get; set; }      

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }

        public string? SortBy { get; set; }      
        public string? SortDir { get; set; }      

        public int Limit { get; set; } = 50;      
        public int Offset { get; set; } = 0;    
    }
}