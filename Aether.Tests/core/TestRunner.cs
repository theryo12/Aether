using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Aether.Tests.Utils;

namespace Aether.Tests.Core;

// TODO: docs
// maybe make it show memory usage and memory leaks?
// also make it show results at the end of testing.

public class TestRunner
{
    private readonly ConcurrentDictionary<string, List<string>> _results = new();

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
                Run(instance, type.Name, setup, test, teardown);
            }
        });

        PrintResults();
    }

    private void PrintResults()
    {
        foreach (var kvp in _results)
        {
            Console.WriteLine($"{kvp.Key}");
            foreach (var result in kvp.Value)
            {
                Console.WriteLine($"  {result}");
            }
        }
    }

    private void Run(object? instance, string className, MethodInfo? setup, MethodInfo test, MethodInfo? teardown)
    {
        var logBuffer = ArrayPool<char>.Shared.Rent(256);
        try
        {
            setup?.Invoke(instance, null);

            var sw = Stopwatch.StartNew();
            test.Invoke(instance, null);
            sw.Stop();

            FormatLog(logBuffer, "[   OK   ] ", test.Name, sw.ElapsedMilliseconds, className);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            FormatLog(logBuffer, "[ FAILED ] ", test.Name, 0, className, errorMessage);
        }
        finally
        {
            teardown?.Invoke(instance, null);
            ArrayPool<char>.Shared.Return(logBuffer);
        }
    }

    private void FormatLog(char[] buffer, string status, string testName, long duration, string className, string? error = null)
    {
        var sb = new ValueStringBuilder(buffer);
        sb.Append(status);
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

        var formattedLog = sb.ToString();

        _results.AddOrUpdate(
            className,
            _ => new List<string> { formattedLog },
            (_, list) =>
            {
                list.Add(formattedLog);
                return list;
            });
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