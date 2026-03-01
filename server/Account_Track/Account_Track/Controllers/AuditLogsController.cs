using Account_Track.DTOs;
using Account_Track.DTOs.AuditLogDto;
using Account_Track.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _service;
        public AuditLogsController(IAuditLogService service) => _service = service;

        // ======================================================
        // LIST AUDIT LOGS  (Uses usp_AuditLog LIST)
        // ======================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQueryDto query)
        {
            var traceId = HttpContext.TraceIdentifier;

            var (items, pagination) = await _service.GetAsync(query);

            var response = new ApiResponseWithPagination<List<AuditLogDto>>
            {
                Success = true,
                Data = items,
                Pagination = pagination,
                Message = "Fetched audit logs successfully.",
                TraceId = traceId
            };

            return Ok(response);
        }

        // ======================================================
        // GET AUDIT LOG BY ID (Uses usp_AuditLog GET_BY_ID)
        // ======================================================
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuditLogById([FromRoute] int id)
        {
            var traceId = HttpContext.TraceIdentifier;

            var item = await _service.GetByIdSpAsync(id);

            if (item is null)
            {
                return NotFound(new ErrorResponseDto
                {
                    ErrorCode = "AUDIT_LOG_NOT_FOUND",
                    Message = $"Audit log with id {id} was not found.",
                    TraceId = traceId
                });
            }

            return Ok(new ApiResponseDto<AuditLogDto>
            {
                Success = true,
                Data = item,
                Message = "Fetched audit log successfully.",
                TraceId = traceId
            });
        }
    }
}