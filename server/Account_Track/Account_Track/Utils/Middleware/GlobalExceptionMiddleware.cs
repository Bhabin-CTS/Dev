using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Account_Track.DTOs;

namespace Account_Track.Utils.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {

            var traceId = Guid.NewGuid().ToString();

            try
            {
                context.Items["TraceId"] = traceId;
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception");

                await HandleException(context, ex, traceId);
            }
        }


        private static async Task HandleException(
            HttpContext context,
            Exception ex,
            string traceId)
        {
            var response = new ErrorResponseDto
            {
                TraceId = traceId,
                ErrorCode = string.Empty,
                Message = string.Empty,
            };

            switch (ex)
            {
                // ================= BUSINESS ERRORS =================
                case BusinessException be:
                    context.Response.StatusCode = 400;
                    response.ErrorCode = be.ErrorCode;
                    response.Message = be.Message;
                    break;

                // ================= VALIDATION =================
                case ValidationException ve:
                    context.Response.StatusCode = 400;
                    response.ErrorCode = "INVALID_REQUEST";
                    response.Message = ve.Message;
                    break;

                // ================= AUTH =================
                case UnauthorizedAccessException:
                    context.Response.StatusCode = 401;
                    response.ErrorCode = "UNAUTHORIZED";
                    response.Message = "Unauthorized access";
                    break;

                // ================= NOT FOUND =================
                case KeyNotFoundException knf:
                    context.Response.StatusCode = 404;
                    response.ErrorCode = "NOT_FOUND";
                    response.Message = knf.Message;
                    break;

                // ================= SQL ERRORS =================
                case SqlException sqlEx:
                    context.Response.StatusCode = 500;
                    response.ErrorCode = "DATABASE_ERROR";
                    response.Message = "Database operation failed";
                    break;

                // ================= ArgumentException =================
                case ArgumentException ae:
                    context.Response.StatusCode = 400;
                    response.ErrorCode = "INVALID_ARGUMENT";
                    response.Message = ae.Message;
                    break;

                // ================= NullReference =================
                case NullReferenceException:
                    context.Response.StatusCode = 500;
                    response.ErrorCode = "NULL_REFERENCE";
                    response.Message = "Unexpected null value encountered";
                    break;

                // ================= FALLBACK =================
                default:
                    context.Response.StatusCode = 500;
                    response.ErrorCode = "INTERNAL_SERVER_ERROR";
                    response.Message = "Unexpected error occurred";
                    break;
            }

            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
