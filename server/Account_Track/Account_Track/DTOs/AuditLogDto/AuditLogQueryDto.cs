using System;

namespace Account_Track.DTOs.AuditLogDto
{

    public class AuditLogQueryDto
    {
        public int? UserId { get; set; }
        public int? LoginId { get; set; }
        public string? EntityType { get; set; }     
        public int? EntityId { get; set; }
        public string? Action { get; set; }          
        public string? SearchText { get; set; }      
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "DESC";  
        public int Limit { get; set; } =20;
        public int Offset { get; set; } = 0;
    }
}