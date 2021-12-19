using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class ThreadSafeSetTest
    {
        private ThreadSafeSet<string> set;

        [TestInitialize]
        public void Setup()
        {
            set = new ThreadSafeSet<string>();
            for (int i = 0; i < 10; i++)
            {
                set.Add($"test {i}");
            }
        }

        [TestMethod]
        public void Remove()
        {
            set.Should().Contain("test 0");
            set.Remove("test 0");
            set.Should().NotContain("test 0");
        }

        [TestMethod]
        public void Contains()
        {
            set.Contains("test 1").Should().BeTrue();
        }

        [TestMethod]
        public void Except()
        {
            string[] exclude = { "test 0", "test 5", "test 9" };
            set.Should().Contain(exclude);
            set.ExceptWith(exclude);
            set.Should().NotContain(exclude);
        }
    }
}