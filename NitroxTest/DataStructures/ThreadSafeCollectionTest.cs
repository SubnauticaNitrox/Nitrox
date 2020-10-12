using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;

namespace NitroxTest.DataStructures
{
    [TestClass]
    public class ThreadSafeCollectionTest
    {
        private ThreadSafeCollection<string> list;
        private ThreadSafeCollection<string> set;

        [TestInitialize]
        public void Setup()
        {
            list = new ThreadSafeCollection<string>();
            set = new ThreadSafeCollection<string>(new HashSet<string>());
            for (int i = 0; i < 10; i++)
            {
                set.Add($"test {i}");
                list.Add($"test {i}");
            }
        }

        [TestMethod]
        public void IsSet()
        {
            set.IsSet.Should().BeTrue();
            list.IsSet.Should().BeFalse();
        }

        [TestMethod]
        public void Insert()
        {
            list.Insert(5, "derp");
            list[5].Should().Be("derp");
            list[0] = "Hello world!";
            list[0].Should().Be("Hello world!");

            set.Insert(8, "wot");
            set[8].Should().Be("wot");
            set[set.Count - 1] = "Hello world!";
            set[set.Count - 1].Should().Be("Hello world!");
        }

        [TestMethod]
        public void RemoveAt()
        {
            list.RemoveAt(5);
            foreach (string item in list)
            {
                item.Should().NotBe("test 5");
            }

            set.RemoveAt(5);
            foreach (string item in set)
            {
                item.Should().NotBe("test 5");
            }
        }

        [TestMethod]
        public void Remove()
        {
            list.Remove("test 0");
            list[0].Should().Be("test 1");

            set.Remove("test 0");
            set[0].Should().Be("test 1");
        }

        [TestMethod]
        public void Find()
        {
            list.Find(s => s == "test 1").Should().Be("test 1");
            list.Find(s => s == "tesT 1").Should().BeNull();
            list.Find(s => s == "test 1361").Should().BeNull();
            
            set.Find(s => s == "test 7").Should().Be("test 7");
            set.Find(s => s == "tesT 7").Should().BeNull();
            set.Find(s => s == "test 1361").Should().BeNull();
        }
    }
}
