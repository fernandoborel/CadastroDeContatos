using FluentValidation;

namespace Contatos.Api.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            await HandleException(context, e);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var message = "Erro interno de servidor";

        if (ex is ValidationException validationEx)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = validationEx.Errors.First().ErrorMessage;
        }
        else if (ex is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = ex.Message;
        }
        else if (ex is ApplicationException)
        {
            statusCode = StatusCodes.Status422UnprocessableEntity;
            message = ex.Message;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            error = message,
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
