using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Aether.Tests.Utils;

namespace Aether.Tests.Core;

public class TestRunner
{
    private readonly ConcurrentQueue<string> _results = new();

    private readonly ConcurrentDictionary<Type, (MethodInfo? setup, MethodInfo? teardown, MethodInfo[] Tests)> _testCache = [];

    public TestRunner()
    {
        CacheTestMethods();
    }

    public void RunAll()
    {
        Parallel.ForEach(_testCache, kvp =>
        {
            var (type, methods) = kvp;
            var (setup, teardown, tests) = methods;

            var instance = Activator.CreateInstance(type);

            foreach (var test in tests.AsSpan())
            {
                Run(instance, setup, test, teardown);
            }
        });

        PrintResults();
    }

    private void PrintResults()
    {
        foreach (var result in _results)
        {
            Console.WriteLine(result);
        }
    }

    private void Run(object? instance, MethodInfo? setup, MethodInfo test, MethodInfo? teardown)
    {
        var logBuffer = ArrayPool<char>.Shared.Rent(256);
        try
        {
            setup?.Invoke(instance, null);

            var sw = Stopwatch.StartNew();
            test.Invoke(instance, null);
            sw.Stop();

            FormatLog(logBuffer, "PASSED", test.Name, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            FormatLog(logBuffer, "FAILED", test.Name, 0, ex.Message);
        }
        finally
        {
            teardown?.Invoke(instance, null);
            ArrayPool<char>.Shared.Return(logBuffer);
        }
    }

    private void FormatLog(char[] buffer, string status, string testName, long duration, string? error = null)
    {
        var sb = new ValueStringBuilder(buffer);
        sb.Append(status);
        sb.Append(": ");
        sb.Append(testName);

        if (duration > 0)
        {
            sb.Append(" in ");
            sb.Append(duration.ToString());
            sb.Append(" ms");
        }

        if (error != null)
        {
            sb.Append(" - ");
            sb.Append(error);
        }

        _results.Enqueue(sb.ToString());
    }

    private void CacheTestMethods()
    {
        var testClasses = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.GetMethods()
            .Any(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0));
        // comparing to length instead of .Any() "both for clarity and for performace"

        foreach (var type in testClasses)
        {
            var setup = FindMethodWithAttribute(type, typeof(SetupAttribute));
            var teardown = FindMethodWithAttribute(type, typeof(TeardownAttribute));
            var tests = FindMethodsWithAttribute(type, typeof(TestAttribute));

            _testCache[type] = (setup, teardown, tests);
        }
    }

    private static MethodInfo? FindMethodWithAttribute(Type type, Type attributeType)
    {
        return type.GetMethods().FirstOrDefault(m => m.GetCustomAttributes(attributeType, false).Length > 0);
    }

    private static MethodInfo[] FindMethodsWithAttribute(Type type, Type attributeType)
    {
        return type.GetMethods().Where(m => m.GetCustomAttributes(attributeType, false).Length > 0).ToArray();
    }
}