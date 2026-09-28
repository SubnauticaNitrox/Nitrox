namespace Nitrox.Model.DataStructures;

[TestClass]
public class ExpandableRingBufferTest
{
    [TestMethod]
    public void ShouldAddItemsAndIncreaseCount()
    {
        ExpandableRingBuffer<string> buffer = new(5);

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
        ExpandableRingBuffer<string> buffer = new(2) { "1", "2" };
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
        ExpandableRingBuffer<int> buffer = new(3) { 1, 2, 3 };
        buffer.RemoveFirst();

        buffer.Add(4);
        // buffer is [4, 2, 3] internally
        buffer.Add(5);
        // buffer is [2, 3, 4, 5] (after expansion)

        buffer.Count.Should().Be(4);
        buffer.First.Should().Be(2);
        buffer.Last.Should().Be(5);
    }

    [TestMethod]
    public void ShouldRemoveFirstCorrectly()
    {
        ExpandableRingBuffer<string> buffer = new(3) { "1", "2", "3" };

        buffer.RemoveFirst();

        buffer.Count.Should().Be(2);
        buffer.First.Should().Be("2");
        buffer.Last.Should().Be("3");
    }

    [TestMethod]
    public void ShouldRemoveLastCorrectly()
    {
        ExpandableRingBuffer<string> buffer = new(3) { "1", "2", "3" };

        buffer.RemoveLast();

        buffer.Count.Should().Be(2);
        buffer.First.Should().Be("1");
        buffer.Last.Should().Be("2");
    }

    [TestMethod]
    public void ShouldBeEmptyWhenCleared()
    {
        ExpandableRingBuffer<string> buffer = new(10) { "1", "2", "3" };

        buffer.Count.Should().Be(3);

        buffer.Clear();

        buffer.Count.Should().Be(0);
        buffer.IsEmpty().Should().BeTrue();
    }

    [TestMethod]
    public void ShouldThrowWhenRemovingFromEmptyBuffer()
    {
        ExpandableRingBuffer<int> buffer = [];

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
        ExpandableRingBuffer<int> buffer = [];

        Action actFirst = () => { int _ = buffer.First; };
        actFirst.Should().Throw<InvalidOperationException>()
            .WithMessage("Buffer is empty");

        Action actLast = () => { int _ = buffer.Last; };
        actLast.Should().Throw<InvalidOperationException>()
            .WithMessage("Buffer is empty");
    }

    [TestMethod]
    public void ShouldAccessItemsByLogicalIndexAndWrapInternally()
    {
        // Tests that logical indices 0-Count work perfectly even when the underlying array is wrapped
        ExpandableRingBuffer<int> buffer = new(4) { 10, 20, 30, 40 };

        buffer.RemoveFirst();
        buffer.RemoveFirst();
        // Internal array has empty slots at index 0 and 1. Head is at 2.

        buffer.Add(50);
        buffer.Add(60);
        // Internal array physically wraps: [50, 60, 30, 40]

        // Logical indexer should seamlessly abstract this away
        buffer.Should().HaveElementAt(0, 30);
        buffer.Should().HaveElementAt(1, 40);
        buffer.Should().HaveElementAt(2, 50);
        buffer.Should().HaveElementAt(3, 60);
    }

    [TestMethod]
    public void ShouldThrowWhenAccessingOutOfBoundsIndex()
    {
        ExpandableRingBuffer<int> buffer = new(4) { 10, 20, 30 };

        Action actNegative = () => { int _ = buffer[-1]; };
        actNegative.Should().Throw<ArgumentOutOfRangeException>();

        Action actTooLarge = () => { int _ = buffer[3]; };
        actTooLarge.Should().Throw<ArgumentOutOfRangeException>();
    }
}
