namespace Aether.Tests.Core;

// TODO: docs
public static class Assert
{
    public static void That(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new AssertException(message ?? "Assertion Failed: Condition is not true.");
        }
    }

    public static void Throws<TException>(Action action, string? message = null) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new AssertException(
                message ?? $"Assertion Failed: Expected exception of type {typeof(TException)}, but got {ex.GetType()}."
            );
        }

        throw new AssertException(message ?? $"Assertion Failed: Expected exception of type {typeof(TException)}, but no exception was thrown.");
    }
}

public class AssertException(string message) : Exception(message) { }