namespace AxisCore.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests.
/// Requires validators to be registered as IValidator&lt;TRequest&gt; in the service collection.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">Request validators</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}

/// <summary>
/// Validator interface for request validation.
/// </summary>
/// <typeparam name="T">Type to validate</typeparam>
public interface IValidator<T>
{
    /// <summary>
    /// Validates the instance.
    /// </summary>
    /// <param name="context">Validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellationToken);
}

/// <summary>
/// Validation context.
/// </summary>
/// <typeparam name="T">Type being validated</typeparam>
public class ValidationContext<T>
{
    /// <summary>
    /// Gets the instance being validated.
    /// </summary>
    public T InstanceToValidate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationContext{T}"/> class.
    /// </summary>
    /// <param name="instanceToValidate">Instance to validate</param>
    public ValidationContext(T instanceToValidate)
    {
        InstanceToValidate = instanceToValidate;
    }
}

/// <summary>
/// Validation result.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public List<ValidationFailure> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    public ValidationResult()
    {
        Errors = new List<ValidationFailure>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="errors">Validation errors</param>
    public ValidationResult(IEnumerable<ValidationFailure> errors)
    {
        Errors = errors.ToList();
    }
}

/// <summary>
/// Validation failure.
/// </summary>
public class ValidationFailure
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailure"/> class.
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <param name="errorMessage">Error message</param>
    public ValidationFailure(string propertyName, string errorMessage)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IEnumerable<ValidationFailure> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">Validation errors</param>
    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }
}
