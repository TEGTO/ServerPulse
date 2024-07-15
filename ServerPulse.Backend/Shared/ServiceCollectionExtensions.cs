using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Shared.Middlewares;

namespace Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCustomInvalidModelStateResponseContollers(this IServiceCollection services)
        {
            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value.ValidationState == ModelValidationState.Invalid)
                        .SelectMany(x => x.Value.Errors.Select(e => new ValidationFailure(x.Key, e.ErrorMessage)))
                        .ToList();
                    throw new ValidationException(errors);
                };
            });
            return services;
        }
        public static IServiceCollection AddSharedFluentValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<ExceptionMiddleware>();
            return services;
        }
    }
}
