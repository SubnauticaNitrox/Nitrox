using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures;

namespace NitroxTest.Model
{
    [TestClass]
    public class PriorityQueueTest
    {
        [TestMethod]
        public void SameOrder()
        {
            PriorityQueue<string> queue = new PriorityQueue<string>();
            queue.Enqueue(0, "First");
            queue.Enqueue(0, "Second");
            queue.Enqueue(0, "Third");

            Assert.AreEqual("First", queue.Dequeue());
            Assert.AreEqual("Second", queue.Dequeue());
            Assert.AreEqual("Third", queue.Dequeue());
        }

        [TestMethod]
        public void DifferentOrder()
        {
            PriorityQueue<string> queue = new PriorityQueue<string>();
            queue.Enqueue(3, "First");
            queue.Enqueue(2, "Second");
            queue.Enqueue(1, "Third");

            Assert.AreEqual("First", queue.Dequeue());
            Assert.AreEqual("Second", queue.Dequeue());
            Assert.AreEqual("Third", queue.Dequeue());
        }

        [TestMethod]
        public void SomeAreSameOrder()
        {
            PriorityQueue<string> queue = new PriorityQueue<string>();
            queue.Enqueue(2, "First");
            queue.Enqueue(2, "Second");
            queue.Enqueue(0, "Third");

            Assert.AreEqual("First", queue.Dequeue());
            Assert.AreEqual("Second", queue.Dequeue());
            Assert.AreEqual("Third", queue.Dequeue());
        }

        [TestMethod]
        public void PrioritySanity()
        {
            PriorityQueue<string> queue = new PriorityQueue<string>();
            queue.Enqueue(2, "Second");
            queue.Enqueue(3, "First");
            queue.Enqueue(1, "Third");

            Assert.AreEqual("First", queue.Dequeue());
            Assert.AreEqual("Second", queue.Dequeue());
            Assert.AreEqual("Third", queue.Dequeue());
        }

        [TestMethod]
        public void CountSanity()
        {
            PriorityQueue<string> queue = new PriorityQueue<string>();
            queue.Enqueue(2, "Second");
            queue.Enqueue(3, "First");
            queue.Enqueue(1, "Third");

            Assert.AreEqual(3, queue.Count);

            queue.Dequeue();
            queue.Dequeue();
            
            Assert.AreEqual(1, queue.Count);
        }
    }
}
