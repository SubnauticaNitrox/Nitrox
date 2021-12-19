using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class ThreadSafeQueueTest
    {
        private ThreadSafeQueue<string> queue;

        [TestInitialize]
        public void Setup()
        {
            queue = new ThreadSafeQueue<string>();
            for (int i = 0; i < 10; i++)
            {
                queue.Enqueue($"test {i}");
            }
        }

        [TestMethod]
        public void Peek()
        {
            queue.Peek().Should().Be("test 0");
        }

        [TestMethod]
        public void Enqueue()
        {
            queue.Enqueue("derp");
            queue.Count.Should().Be(11);
        }

        [TestMethod]
        public void Dequeue()
        {
            queue.Dequeue().Should().Be("test 0");
            queue.Count.Should().Be(9);
            queue.Should().NotContain("test 0");
        }

        [TestMethod]
        public void Clear()
        {
            queue.Clear();
            queue.Count.Should().Be(0);
        }

        [TestMethod]
        public void Contains()
        {
            queue.Contains("test 5").Should().BeTrue();
            queue.Contains("test 11").Should().BeFalse();
        }
    }
}
