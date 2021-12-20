using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class ThreadSafeListTest
    {
        private ThreadSafeList<string> list;

        [TestInitialize]
        public void Setup()
        {
            list = new ThreadSafeList<string>();
            for (int i = 0; i < 10; i++)
            {
                list.Add($"test {i}");
            }
        }

        [TestMethod]
        public void Insert()
        {
            list.Insert(5, "derp");
            list[5].Should().Be("derp");
            list[0] = "Hello world!";
            list[0].Should().Be("Hello world!");
        }

        [TestMethod]
        public void RemoveAt()
        {
            list.RemoveAt(5);
            foreach (string item in list)
            {
                item.Should().NotBe("test 5");
            }
        }

        [TestMethod]
        public void Remove()
        {
            list.Remove("test 0");
            list[0].Should().Be("test 1");
        }

        [TestMethod]
        public void Find()
        {
            list.Find(s => s == "test 1").Should().Be("test 1");
            list.Find(s => s == "tesT 1").Should().BeNull();
            list.Find(s => s == "test 1361").Should().BeNull();
        }
    }
}