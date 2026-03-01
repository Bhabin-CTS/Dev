using System.Collections.Generic;
using System.Threading.Tasks;
using Account_Track.DTOs;

namespace Account_Track.Services
{
    public interface IAuditLogService
    {
        // LIST (SP)
        Task<(List<AuditLogDto> Items, PaginationDto Pagination)> GetAsync(AuditLogQueryDto query);

        // GET BY ID using SP
        Task<AuditLogDto?> GetByIdSpAsync(int id, string? action = null);

    }
}