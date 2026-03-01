using Account_Track.Dtos.BranchDto;
using Account_Track.DTOs;
using Account_Track.DTOs.BranchDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")] 
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _service;

        public BranchesController(IBranchService service)
        {
            _service = service;
        }

        // POST v1/branches (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequestDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")!.Value);
                int loginId = int.Parse(User.FindFirst("LoginId")!.Value);
                var data = await _service.CreateBranchAsync(dto, userId, loginId);

                return StatusCode(201, new ApiResponseDto<BranchResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Branch created successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex) when (ex.ErrorCode == "DUPLICATE_IFSC")
            {
                return StatusCode(409, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_IFSC",
                    Message = "IFSCCode already exists",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException)
            {
                return Conflict(new ErrorResponseDto
                {
                    ErrorCode = "DUPLICATE_IFSC",
                    Message = "IFSCCode already exists",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // PUT v1/branches/{id} (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBranch([FromRoute] int id, [FromBody] UpdateBranchRequestDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")!.Value);
                int loginId = int.Parse(User.FindFirst("LoginId")!.Value);
                var data = await _service.UpdateBranchAsync(id, dto, userId, loginId);

                return Ok(new ApiResponseDto<BranchResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Branch updated successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "BRANCH_NOT_FOUND",
                    Message = "branchId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex) when (ex.ErrorCode == "DUPLICATE_IFSC")
            {
                return StatusCode(409, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_IFSC",
                    Message = "IFSCCode already exists",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627 || ex.Number == 50010)
            {
                return StatusCode(409, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_IFSC",
                    Message = "IFSCCode already exists",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // GET v1/branches (Officer/Manager/Admin)
        [HttpGet]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetBranches([FromQuery] GetBranchesRequestDto request)
        {
            try
            {
                var (data, pagination) = await _service.GetBranchesAsync(request);

                return Ok(new ApiResponseWithPagination<object>
                {
                    Success = true,
                    Data = data,
                    Pagination = pagination,
                    Message = "Branches retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex) when (ex.ErrorCode == "INVALID_REQUEST")
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // GET v1/branches/{id} (Officer/Manager/Admin)
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetBranchDetail([FromRoute] int id)
        {
            try
            {

                var data = await _service.GetBranchByIdAsync(id);

                return Ok(new ApiResponseDto<BranchResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Branch details retrieved",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "BRANCH_NOT_FOUND",
                    Message = "branchId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}