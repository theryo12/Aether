namespace Aether.Tests.Core;

// TODO: docs
// make areequal, istrue one method
public static class Assert
{
    public static void AreEqual<T>(T expected, T actual, string? message = null) where T : IEquatable<T>
    {
        if (!expected.Equals(actual))
        {
            throw new AssertException(
                message ?? $"Assertion Failed: Expected <{expected}>, but got <{actual}>."
            );
        }
    }

    public static void AreEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual, string? message = null)
    where T : IEquatable<T>
    {
        if (expected.Length != actual.Length)
        {
            throw new AssertException(message ?? $"Assertion Failed: Array lengths differ. Expected {expected.Length}, but got {actual.Length}.");
        }

        for (int i = 0; i < expected.Length; i++)
        {
            if (!expected[i].Equals(actual[i]))
            {
                throw new AssertException(message ?? $"Assertion Failed: Arrays differ at index {i}. Expected <{expected[i]}>, but got <{actual[i]}>.");
            }
        }
    }

    public static void IsTrue(bool condition, string? message = null)
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