namespace Account_Track.DTOs.AuditLogDto
{
    public class AuditLogGetByIdSpResultDto
    {
        public int AuditLogId { get; set; }
        public int UserId { get; set; }
        public int LoginId { get; set; }
        public string EntityType { get; set; } = default!;
        public int EntityId { get; set; }
        public string Action { get; set; } = default!;
        public string? beforeState { get; set; }
        public string afterState { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string? ChangedByName { get; set; }
        public int ChangedByRoleId { get; set; }
    }
}
