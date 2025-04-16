namespace XRPL.NET.Core.Utils;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
public class Result
{
    private readonly List<string> _errors = new();
    
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => !_errors.Any();
    
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Gets the errors that occurred during the operation.
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    
    /// <summary>
    /// Gets the first error message or null if the operation was successful.
    /// </summary>
    public string? Error => IsFailure ? _errors[0] : null;
    
    /// <summary>
    /// Creates a new successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new Result();
    
    /// <summary>
    /// Creates a new successful result with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A successful result with a value.</returns>
    public static Result<T> Success<T>(T value) => new Result<T>(value);
    
    /// <summary>
    /// Creates a new failed result with the specified error.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(string error)
    {
        var result = new Result();
        result._errors.Add(error);
        return result;
    }
    
    /// <summary>
    /// Creates a new failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(IEnumerable<string> errors)
    {
        var result = new Result();
        result._errors.AddRange(errors);
        return result;
    }
    
    /// <summary>
    /// Creates a new failed result with a value and the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result with a value.</returns>
    public static Result<T> Failure<T>(string error)
    {
        var result = new Result<T>(default);
        result._errors.Add(error);
        return result;
    }
    
    /// <summary>
    /// Creates a new failed result with a value and the specified errors.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed result with a value.</returns>
    public static Result<T> Failure<T>(IEnumerable<string> errors)
    {
        var result = new Result<T>(default);
        result._errors.AddRange(errors);
        return result;
    }
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    internal Result(T? value)
    {
        _value = value;
    }
    
    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the operation failed.</exception>
    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException($"Cannot access value of failed result. Errors: {string.Join(", ", Errors)}");
    
    /// <summary>
    /// Tries to get the value of the result.
    /// </summary>
    /// <param name="value">When this method returns, contains the value of the result if the operation succeeded, or the default value of <typeparamref name="T"/> if the operation failed.</param>
    /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(out T? value)
    {
        value = IsSuccess ? _value : default;
        return IsSuccess;
    }
}

/// <summary>
/// Provides extension methods for the Result class.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes the specified action if the result is successful.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <returns>The original result.</returns>
    public static Result OnSuccess(this Result result, Action onSuccess)
    {
        if (result.IsSuccess)
        {
            onSuccess();
        }
        return result;
    }
    
    /// <summary>
    /// Executes the specified action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <returns>The original result.</returns>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> onSuccess)
    {
        if (result.IsSuccess)
        {
            onSuccess(result.Value);
        }
        return result;
    }
    
    /// <summary>
    /// Executes the specified action if the result failed.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="onFailure">The action to execute if the result failed.</param>
    /// <returns>The original result.</returns>
    public static Result OnFailure(this Result result, Action<IReadOnlyList<string>> onFailure)
    {
        if (result.IsFailure)
        {
            onFailure(result.Errors);
        }
        return result;
    }
    
    /// <summary>
    /// Executes the specified action if the result failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onFailure">The action to execute if the result failed.</param>
    /// <returns>The original result.</returns>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<IReadOnlyList<string>> onFailure)
    {
        if (result.IsFailure)
        {
            onFailure(result.Errors);
        }
        return result;
    }
    
    /// <summary>
    /// Maps the result to a new result with a different value type.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="mapper">The function to map the value.</param>
    /// <returns>A new result with the mapped value if the original result was successful; otherwise, a failed result.</returns>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Errors);
    }
    
    /// <summary>
    /// Binds the result to a new result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="binder">The function to bind the value.</param>
    /// <returns>The bound result if the original result was successful; otherwise, a failed result.</returns>
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TOut>(result.Errors);
    }
}