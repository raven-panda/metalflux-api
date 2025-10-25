using System.Net;
using MetalfluxApi.Server.Core.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace MetalfluxApi.Server.Core.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env,
    IConfiguration config
)
{
    private readonly string _baseUrl = env.IsDevelopment()
        ? config["ASPNETCORE_URLS"]?.Split(';')[0] ?? "http://localhost:5146"
        : "https://learnathome.io";

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context); // passe au middleware suivant
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            var (status, title, type) = GetStatus(ex);

            await RpProblemDetailsFactory.WriteProblemDetails(
                context,
                status,
                title,
                ex.Message,
                type
            );
        }
    }

    private (int status, string title, string type) GetStatus(Exception ex)
    {
        return ex switch
        {
            EntityNotFoundException => (
                (int)HttpStatusCode.NotFound,
                "Entity Not Found",
                $"{_baseUrl}/docs/errors/entity-not-found.html"
            ),
            EntityUniqueConstraintViolationException => (
                (int)HttpStatusCode.Conflict,
                "Entity Unique Constraint Violation",
                $"{_baseUrl}/docs/errors/entity-unique-constraint-violation.html"
            ),
            PasswordsNotMatchingException => (
                (int)HttpStatusCode.BadRequest,
                "Passwords Not Matching",
                $"{_baseUrl}/docs/errors/passwords-not-matching.html"
            ),
            SecurityTokenException => (
                (int)HttpStatusCode.Unauthorized,
                "Security Token Invalid",
                $"{_baseUrl}/docs/errors/security-token-invalid.html"
            ),
            InvalidCredentialsException => (
                (int)HttpStatusCode.Unauthorized,
                "Invalid Credentials",
                $"{_baseUrl}/docs/errors/invalid-credentials.html"
            ),
            BadHttpRequestException => (
                (int)HttpStatusCode.BadRequest,
                GetTitleFromStatusCode(HttpStatusCode.BadRequest),
                GetTypeFromStatusCode(HttpStatusCode.BadRequest)
            ),
            NotImplementedException => (
                (int)HttpStatusCode.NotImplemented,
                GetTitleFromStatusCode(HttpStatusCode.NotImplemented),
                GetTypeFromStatusCode(HttpStatusCode.NotImplemented)
            ),
            HttpRequestException exception => (
                exception.StatusCode.HasValue
                    ? (int)exception.StatusCode!
                    : (int)HttpStatusCode.InternalServerError,
                GetTitleFromStatusCode(exception.StatusCode),
                GetTypeFromStatusCode(exception.StatusCode)
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                GetTitleFromStatusCode(HttpStatusCode.InternalServerError),
                GetTypeFromStatusCode(HttpStatusCode.InternalServerError)
            ),
        };
    }

    private static string GetTitleFromStatusCode(HttpStatusCode? statusCode)
    {
        statusCode ??= HttpStatusCode.InternalServerError;

        return statusCode.Value switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.NotImplemented => "Not Implemented",
            HttpStatusCode.NotFound => "Not Found",
            _ => "Internal Server Error",
        };
    }

    private static string GetTypeFromStatusCode(HttpStatusCode? statusCode)
    {
        statusCode ??= HttpStatusCode.InternalServerError;

        return statusCode.Value switch
        {
            HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            HttpStatusCode.NotImplemented => "https://tools.ietf.org/html/rfc9110#section-15.6.2",
            HttpStatusCode.InternalServerError =>
                "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            _ => string.Empty,
        };
    }
}
