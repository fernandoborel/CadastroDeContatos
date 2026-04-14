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
        //Por default o erro será tratado como 500 (INTERNAL SERVER ERROR)
        var statusCode = StatusCodes.Status500InternalServerError;
        var message = "Erro interno de servidor";

        //Regras customizadas
        if (ex is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = ex.Message;
        }
        else if (ex is ApplicationException)
        {
            statusCode = StatusCodes.Status422UnprocessableEntity;
            message = ex.Message;
        }

        //Retorna a repsosta em JSON
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            error = ex.Message,
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}