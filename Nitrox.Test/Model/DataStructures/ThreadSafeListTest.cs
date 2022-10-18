using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.DataStructures
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
        
        [TestMethod]
        public void ReadAndWriteSimultaneous()
        {
            int iterations = 500000;

            ThreadSafeList<string> comeGetMe = new(iterations);
            List<long> countsRead = new();
            long addCount = 0;

            Random r = new Random();
            DoReaderWriter(() =>
                {
                    countsRead.Add(Interlocked.Read(ref addCount));
                },
                i =>
                {
                    comeGetMe.Add(new string(Enumerable.Repeat(' ', 10).Select(c => (char)r.Next('A', 'Z')).ToArray()));
                    Interlocked.Increment(ref addCount);
                },
                iterations);

            addCount.Should().Be(iterations);
            countsRead.Count.Should().BeGreaterThan(0);
            countsRead.Last().Should().Be(iterations);
            comeGetMe.Count.Should().Be(iterations);
        }

        [TestMethod]
        public void IterateAndAddSimultaneous()
        {
            int iterations = 500000;

            ThreadSafeList<string> comeGetMe = new(iterations);
            long addCount = 0;
            long iterationsReadMany = 0;

            Random r = new Random();
            DoReaderWriter(() =>
                {
                    foreach (string item in comeGetMe)
                    {
                        item.Length.Should().BeGreaterThan(0);
                        Interlocked.Increment(ref iterationsReadMany);
                    }
                },
                i =>
                {
                    comeGetMe.Add(new string(Enumerable.Repeat(' ', 10).Select(c => (char)r.Next('A', 'Z')).ToArray()));
                    Interlocked.Increment(ref addCount);
                },
                iterations);

            addCount.Should().Be(iterations);
            iterationsReadMany.Should().BePositive();
        }

        [TestMethod]
        public void IterateAndAdd()
        {
            ThreadSafeList<int> nums = new()
            {
                1,2,3,4,5
            };

            foreach (int num in nums)
            {
                if (num == 3)
                {
                    nums.Add(10);
                }
            }

            nums.Count.Should().Be(6);
            nums.Last().Should().Be(10);
        }

        private void DoReaderWriter(Action reader, Action<int> writer, int iterators)
        {
            ManualResetEvent barrier = new(false);
            Thread readerThread = new(() =>
            {
                while (!barrier.SafeWaitHandle.IsClosed)
                {
                    reader();
                    Thread.Yield();
                }

                // Read one last time after writer finishes
                reader();
            });
            Thread writerThread = new(() =>
            {
                for (int i = 0; i < iterators; i++)
                {
                    writer(i);
                }
                barrier.Set(); // Signal done
            });

            readerThread.Start();
            writerThread.Start();
            barrier.WaitOne(); // Wait for signal
        }
    }
}
