using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IPAY.WebApi.Filters;

/// <summary>
/// Runs the registered FluentValidation validator (if any) for every action argument and answers
/// 400 with a ValidationProblemDetails body when a rule fails. Opt-in per controller with
/// <c>[ServiceFilter(typeof(ValidationFilter))]</c>; arguments without a registered validator are skipped.
/// </summary>
public sealed class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(
                    new ValidationProblemDetails(result.ToDictionary()) { Status = StatusCodes.Status400BadRequest });
                return;
            }
        }

        await next();
    }
}
