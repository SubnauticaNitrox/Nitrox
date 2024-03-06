using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nitrox.Test.Polyfill;

[TestClass]
public class PolyfillTest
{
    [TestMethod]
    public void Range()
    {
        string value = "test"[1..];
        value.Should().Be("est");
    }

    [TestMethod]
    public void Index()
    {
        char value = "test"[^2];
        value.Should().Be('s');
    }

    internal class TestClass
    {
        public required string Name { get; init; }

        public TestClass()
        {
        
        }

        [SetsRequiredMembers]
        public TestClass(string name)
        {
            Name = name;
        }

        public string AutomaticName(int number, [CallerArgumentExpression(nameof(number))] string name = "")
        {
            return name;
        }

        [SkipLocalsInit]
        public void Stackalloc()
        {
            _ = stackalloc int[8];
        }

        public void Handler(string name, [InterpolatedStringHandlerArgument(nameof(name))] ref TestHandlerStruct handler)
        {
        }
    }

    internal readonly struct UnscopedRefStruct
    {
        private readonly int number;

        [UnscopedRef]
        public readonly ref readonly int GetRef()
        {
            return ref number;
        }
    }
    [InterpolatedStringHandler]
    internal struct TestHandlerStruct
    {

    }
}
