using System.Text.Json;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, 400,
                type: "https://httpstatuses.com/400",
                title: "Validation error",
                detail: "One or more validation errors occurred.",
                errors: ex.Errors.Select(e => new ErrorDetail
                {
                    Field = ToCamelCase(e.PropertyName),
                    Message = e.ErrorMessage
                }));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain rule violation: {Message}", ex.Message);

            await WriteProblemAsync(context, 422,
                type: "https://httpstatuses.com/422",
                title: "Business rule violation",
                detail: ex.Message,
                errors: [new ErrorDetail { Field = string.Empty, Message = ex.Message }]);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);

            await WriteProblemAsync(context, 404,
                type: "https://httpstatuses.com/404",
                title: "Resource not found",
                detail: ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflict: {Message}", ex.Message);

            await WriteProblemAsync(context, 409,
                type: "https://httpstatuses.com/409",
                title: "Conflict",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Type} — {Message}", ex.GetType().Name, ex.Message);

            var detail = _env.IsDevelopment()
                ? BuildExceptionChain(ex)
                : "An unexpected error occurred. Please try again later.";

            await WriteProblemAsync(context, 500,
                type: "https://httpstatuses.com/500",
                title: "Internal server error",
                detail: detail);
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int status,
        string type,
        string title,
        string detail,
        IEnumerable<ErrorDetail>? errors = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var problem = new ProblemDetailResponse
        {
            Type = type,
            Title = title,
            Status = status,
            Detail = detail,
            Errors = errors?.ToList()
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static string BuildExceptionChain(Exception ex)
    {
        var parts = new List<string>();
        var current = ex;
        while (current is not null)
        {
            parts.Add($"[{current.GetType().Name}] {current.Message}");
            current = current.InnerException;
        }
        return string.Join(" → ", parts);
    }
}

public class ProblemDetailResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public List<ErrorDetail>? Errors { get; set; }
}

public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
