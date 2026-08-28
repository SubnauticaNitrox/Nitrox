namespace Nitrox.Model.DataStructures;

[TestClass]
public class RingBufferTest
{
    [TestMethod]
    public void ShouldAddItemsAndIncreaseCount()
    {
        RingBuffer<string> buffer = new(5);

        buffer.Count.Should().Be(0);
        buffer.Add("1");
        buffer.Count.Should().Be(1);
        buffer.Add("2");
        buffer.Count.Should().Be(2);

        buffer.First.Should().Be("1");
        buffer.Last.Should().Be("2");
    }

    [TestMethod]
    public void ShouldExpandAutomaticallyWhenCapacityReached()
    {
        RingBuffer<string> buffer = new(2);
        buffer.Add("1");
        buffer.Add("2");
        buffer.Count.Should().Be(2);

        // should trigger Expand()
        buffer.Add("3");

        buffer.Count.Should().Be(3);
        buffer.First.Should().Be("1");
        buffer.Last.Should().Be("3");
    }

    [TestMethod]
    public void ShouldPreserveOrderWhenExpandingAfterWrapAround()
    {
        RingBuffer<int> buffer = new(3);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);
        buffer.RemoveFirst();

        buffer.Add(4);
        // buffer is [4, 2, 3]
        buffer.Add(5);
        // buffer is [2, 3, 4, 5] (after expansion)

        buffer.Count.Should().Be(4);
        buffer.First.Should().Be(2);
        buffer.Last.Should().Be(5);
    }

    [TestMethod]
    public void ShouldRemoveFirstCorrectly()
    {
        RingBuffer<string> buffer = new(3);
        buffer.Add("1");
        buffer.Add("2");
        buffer.Add("3");

        buffer.RemoveFirst();

        buffer.Count.Should().Be(2);
        buffer.First.Should().Be("2");
        buffer.Last.Should().Be("3");
        buffer.Head.Should().Be(1);
    }

    [TestMethod]
    public void ShouldRemoveLastCorrectly()
    {
        RingBuffer<string> buffer = new(3);
        buffer.Add("1");
        buffer.Add("2");
        buffer.Add("3");

        buffer.RemoveLast();

        buffer.Count.Should().Be(2);
        buffer.First.Should().Be("1");
        buffer.Last.Should().Be("2");
    }

    [TestMethod]
    public void ShouldBeEmptyWhenCleared()
    {
        RingBuffer<string> buffer = new(10);
        buffer.Add("1");
        buffer.Add("2");
        buffer.Add("3");

        buffer.Count.Should().Be(3);

        buffer.Clear();

        buffer.Count.Should().Be(0);
        buffer.Head.Should().Be(0);
        buffer.Tail.Should().Be(0);
    }

    [TestMethod]
    public void ShouldThrowWhenRemovingFromEmptyBuffer()
    {
        RingBuffer<int> buffer = new();

        Action actRemoveFirst = buffer.RemoveFirst;
        actRemoveFirst.Should().Throw<InvalidOperationException>()
            .WithMessage("Can't remove an element from an empty ring buffer");

        Action actRemoveLast = buffer.RemoveLast;
        actRemoveLast.Should().Throw<InvalidOperationException>()
            .WithMessage("Can't remove an element from an empty ring buffer");
    }

    [TestMethod]
    public void ShouldThrowWhenAccessingFirstOrLastOnEmptyBuffer()
    {
        RingBuffer<int> buffer = new();

        Action actFirst = () => { int _ = buffer.First; };
        actFirst.Should().Throw<InvalidOperationException>()
            .WithMessage("Buffer is empty");

        Action actLast = () => { int _ = buffer.Last; };
        actLast.Should().Throw<InvalidOperationException>()
            .WithMessage("Buffer is empty");
    }

    [TestMethod]
    public void ShouldAccessItemsByWrappedIndex()
    {
        RingBuffer<int> buffer = new(4);
        buffer.Add(10);
        buffer.Add(20);
        buffer.Add(30);

        // regular indexer
        buffer[0].Should().Be(10);
        buffer[1].Should().Be(20);
        buffer[2].Should().Be(30);

        // indexer wraps index with capacity
        buffer[4].Should().Be(10);
        buffer[5].Should().Be(20);
    }
}
