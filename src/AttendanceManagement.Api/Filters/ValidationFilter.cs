using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AttendanceManagement.Api.Filters;

/// <summary>
/// Roda o FluentValidation do DTO de entrada antes da action. Falha vira 400 com
/// ProblemDetails (campo → mensagem); DTO sem validator apenas segue adiante. As
/// invariantes do domínio continuam sendo a última linha de defesa.
/// </summary>
internal sealed class ValidationFilter(
    IServiceProvider serviceProvider,
    ProblemDetailsFactory problemDetailsFactory) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            if (serviceProvider.GetService(typeof(IValidator<>).MakeGenericType(argument.GetType())) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            if (result.IsValid)
            {
                continue;
            }

            foreach (var failure in result.Errors)
            {
                context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }

            var problem = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }
}
