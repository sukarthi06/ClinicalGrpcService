namespace ClinicalService.Api.Validation;

public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) =>
        builder
            .AddEndpointFilter<ValidationFilter<T>>()
            .ProducesValidationProblem();
}
