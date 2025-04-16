using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace XRPL.NET.Core.Utils;

/// <summary>
/// Provides methods for parameter validation with concise syntax.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures that the specified value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The value if not null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    [return: NotNull]
    public static T NotNull<T>([NotNull] T? value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        return value ?? throw new ArgumentNullException(paramName);
    }
    
    /// <summary>
    /// Ensures that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The string if not null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is null or empty.</exception>
    [return: NotNull]
    public static string NotNullOrEmpty([NotNull] string? value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        }
        
        return value;
    }
    
    /// <summary>
    /// Ensures that the specified string is not null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The string if not null, empty, or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is null, empty, or whitespace.</exception>
    [return: NotNull]
    public static string NotNullOrWhiteSpace([NotNull] string? value, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
        }
        
        return value;
    }
    
    /// <summary>
    /// Ensures that the specified collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The collection if not null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when the collection is null or empty.</exception>
    [return: NotNull]
    public static ICollection<T> NotNullOrEmpty<T>([NotNull] ICollection<T>? collection, 
        [CallerArgumentExpression(nameof(collection))] string? paramName = null)
    {
        if (collection == null || collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be null or empty.", paramName);
        }
        
        return collection;
    }
    
    /// <summary>
    /// Ensures that the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The message to include in the exception if the condition is false.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <exception cref="ArgumentException">Thrown when the condition is false.</exception>
    public static void IsTrue(bool condition, string message, 
        [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if (!condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }
    
    /// <summary>
    /// Ensures that the specified value is within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The value if within range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is out of range.</exception>
    public static T InRange<T>(T value, T min, T max, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, 
                $"Value must be between {min} and {max}.");
        }
        
        return value;
    }
    
    /// <summary>
    /// Ensures that the specified value is greater than the specified minimum.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (exclusive).</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The value if greater than the minimum.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not greater than the minimum.</exception>
    public static T GreaterThan<T>(T value, T min, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        if (value.CompareTo(min) <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, 
                $"Value must be greater than {min}.");
        }
        
        return value;
    }
    
    /// <summary>
    /// Ensures that the specified value is greater than or equal to the specified minimum.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <returns>The value if greater than or equal to the minimum.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than the minimum.</exception>
    public static T GreaterThanOrEqual<T>(T value, T min, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) 
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, 
                $"Value must be greater than or equal to {min}.");
        }
        
        return value;
    }
}