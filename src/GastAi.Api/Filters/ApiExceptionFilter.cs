using GastAi.Application.Exceptions;
using GastAi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GastAi.Api.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException notFound:
                context.Result = new NotFoundObjectResult(new ProblemDetails { Title = notFound.Message, Status = 404 });
                context.ExceptionHandled = true;
                break;

            case DomainException domain:
                context.Result = new BadRequestObjectResult(new ProblemDetails { Title = domain.Message, Status = 400 });
                context.ExceptionHandled = true;
                break;
        }
    }
}
