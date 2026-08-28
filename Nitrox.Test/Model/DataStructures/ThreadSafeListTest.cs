namespace Nitrox.Model.DataStructures;

[TestClass]
public class ThreadSafeListTest
{
    private ThreadSafeList<string> list;

    [TestInitialize]
    public void Setup()
    {
        list = [];
        for (int i = 0; i < 10; i++)
        {
            list.Add($"test {i}");
        }
    }

    [TestMethod]
    public void TryGetValue()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list.TryGetValue(i, out string value).Should().BeTrue();
            value.Should().Be($"test {i}");
        }

        list.TryGetValue(list.Count, out _).Should().BeFalse();
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

        ThreadSafeList<int> comeGetMe = new(iterations);
        List<long> countsRead = [];
        long addCount = 0;

        Random r = new();
        DoReaderWriter(() =>
            {
                countsRead.Add(Interlocked.Read(ref addCount));
            },
            i =>
            {
                comeGetMe.Add(r.Next());
                Interlocked.Increment(ref addCount);
            },
            iterations);

        addCount.Should().Be(iterations);
        countsRead.Count.Should().BePositive();
        countsRead.Last().Should().Be(iterations);
        comeGetMe.Should().HaveCount(iterations);
    }

    [TestMethod]
    public void IterateAndAddSimultaneous()
    {
        int iterations = 500000;

        ThreadSafeList<int> comeGetMe = new(iterations);
        long addCount = 0;
        long iterationsReadMany = 0;

        Random r = new();
        DoReaderWriter(() =>
            {
                foreach (int unused in comeGetMe)
                {
                    Interlocked.Increment(ref iterationsReadMany);
                }
            },
            i =>
            {
                comeGetMe.Add(r.Next());
                Interlocked.Increment(ref addCount);
            },
            iterations);

        addCount.Should().Be(iterations);
        iterationsReadMany.Should().BePositive();
    }

    [TestMethod]
    public void IterateAndAdd()
    {
        ThreadSafeList<int> nums =
        [
            1,2,3,4,5
        ];

        foreach (int num in nums)
        {
            if (num == 3)
            {
                nums.Add(10);
            }
        }

        nums.Should().HaveCount(6);
        nums.Last().Should().Be(10);
    }

    private static void DoReaderWriter(Action reader, Action<int> writer, int iterators)
    {
        ManualResetEventSlim barrier = new(false);
        Thread readerThread = new(() =>
        {
            while (!barrier.IsSet)
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
            barrier.Set();
        });

        readerThread.Start();
        writerThread.Start();
        barrier.Wait();
        writerThread.Join();
        readerThread.Join();
    }
}
