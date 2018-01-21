using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxTest.Model
{
    using StringPriorityQueue = NitroxModel.DataStructures.PriorityQueue<string>;

    [TestClass]
    public class PriorityQueueTest
    {
        [TestMethod]
        public void SameOrder()
        {
            StringPriorityQueue queue = new StringPriorityQueue();
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
            StringPriorityQueue queue = new StringPriorityQueue();
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
            StringPriorityQueue queue = new StringPriorityQueue();
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
            StringPriorityQueue queue = new StringPriorityQueue();
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
            StringPriorityQueue queue = new StringPriorityQueue();
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
