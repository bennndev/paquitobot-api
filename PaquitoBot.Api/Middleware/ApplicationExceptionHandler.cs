using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaquitoBot.Application.Common.Exceptions;

namespace PaquitoBot.Api.Middleware;

public class ApplicationExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ApplicationExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            ValidationException validation => new HttpValidationProblemDetails(GroupErrors(validation))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request"
            },
            NotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Detail = exception.Message
            },
            ConflictException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = exception.Message
            },
            _ => null
        };

        if (problem is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });
    }

    private static Dictionary<string, string[]> GroupErrors(ValidationException exception)
    {
        return exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
    }
}
