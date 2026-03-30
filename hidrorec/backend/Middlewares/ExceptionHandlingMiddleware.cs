using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace HidroRec.Backend.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Falha de validacao em {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Os dados enviados sao invalidos.",
                errors = ex.Errors.Select(error => new
                {
                    campo = error.PropertyName,
                    mensagem = error.ErrorMessage
                })
            });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Erro de persistencia em {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Nao foi possivel persistir os dados neste momento."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Acesso negado em {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Falha de negocio em {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Recurso nao encontrado em {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro interno em {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Ocorreu um erro interno. Tente novamente."
            });
        }
    }
}
