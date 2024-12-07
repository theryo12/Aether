namespace Aether.Tests.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestAttribute(string description = "") : Attribute
{
    public string Description { get; } = description;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SetupAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TeardownAttribute : Attribute { }