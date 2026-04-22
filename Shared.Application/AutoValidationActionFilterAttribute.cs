using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shared.Application;

/// <summary>
/// Action filter for Auto-Validation
/// </summary>
public class AutoValidationActionFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Called asynchronously before the action, after model binding is complete.
    /// </summary>
    /// <param name="context">Execution context</param>
    /// <param name="next">Invoked to execute the next action filter or the action itself</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (parameter.BindingInfo == null ||
                (parameter.BindingInfo.BindingSource != BindingSource.Body &&
                 parameter.BindingInfo.BindingSource != BindingSource.Form &&
                 (parameter.BindingInfo.BindingSource != BindingSource.Query ||
                  !parameter.ParameterType.IsClass))) continue;
                
            var model = context.ActionArguments[parameter.Name];
            ArgumentNullException.ThrowIfNull(model);
            
            // Recupero il validatore associato se esiste
            // Faccio questo perché il validatore potrebbe avere necessità di servizi, quindi
            // non posso istanziarlo direttamente, ma devo recuperarlo dal container
            var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
            var serviceProvider = context.HttpContext.RequestServices;
            var validator = serviceProvider.GetService(validatorType) as IValidator;

            if (validator is not null)
            {
                ValidationResult validationResult = validator.Validate(model);
                
                if (validationResult is not null &&
                    !validationResult.IsValid)
                {
                    // Easy management of validation error
                    throw new ArgumentException(
                        validationResult.Errors[0]?.ErrorMessage ?? string.Empty);
                }
            }
        }

        await next();
    }
}