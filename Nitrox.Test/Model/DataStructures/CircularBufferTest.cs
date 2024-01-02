using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.DataStructures;

[TestClass]
public class CircularBufferTest
{
    [TestMethod]
    public void ShouldLimitSizeToMaxSize()
    {
        CircularBuffer<string> buffer = new(1);
        buffer.Count.Should().Be(0);
        buffer.Add("1");
        buffer.Count.Should().Be(1);
        buffer.Add("2");
        buffer.Count.Should().Be(1);

        buffer = new CircularBuffer<string>(5);
        buffer.Count.Should().Be(0);
        buffer.Add("1");
        buffer.Count.Should().Be(1);
        buffer.Add("2");
        buffer.Count.Should().Be(2);
        buffer.Add("3");
        buffer.Count.Should().Be(3);
        buffer.Add("4");
        buffer.Count.Should().Be(4);
        buffer.Add("5");
        buffer.Count.Should().Be(5);
        buffer.Add("6");
        buffer.Count.Should().Be(5);
    }

    [TestMethod]
    public void ShouldOverwriteOldestItemInBufferWhenCapped()
    {
        CircularBuffer<string> buffer = new(3);
        buffer.Add("1");
        buffer[0].Should().Be("1");
        buffer.Add("2");
        buffer[1].Should().Be("2");
        buffer.Add("3");
        buffer[2].Should().Be("3");
        buffer.Add("4");
        buffer[0].Should().Be("4");
        buffer.Add("5");
        buffer[1].Should().Be("5");
        buffer[2].Should().Be("3");
        buffer.Add("6");
        buffer[2].Should().Be("6");
        buffer.Add("7");
        buffer.Should().ContainInOrder("7", "5", "6");
    }

    [TestMethod]
    public void ShouldDiscardAddIfCapacityReached()
    {
        CircularBuffer<string> buffer = new(0);
        buffer.Count.Should().Be(0);
        buffer.Add("1");
        buffer.Count.Should().Be(0);
    }

    [TestMethod]
    public void ShouldBeEmptyWhenCleared()
    {
        CircularBuffer<string> buffer = new(10);
        buffer.Count.Should().Be(0);
        buffer.Add("1");
        buffer.Add("1");
        buffer.Add("1");
        buffer.Count.Should().Be(3);
        buffer.Clear();
        buffer.Count.Should().Be(0);
    }

    [TestMethod]
    public void ShouldGiveLastChanged()
    {
        CircularBuffer<int> buffer = new(3);
        buffer.LastChangedIndex.Should().Be(-1);
        buffer.Add(1);
        buffer.LastChangedIndex.Should().Be(0);
        buffer.Add(2);
        buffer.LastChangedIndex.Should().Be(1);
        buffer.Add(3);
        buffer.LastChangedIndex.Should().Be(2);
        buffer.Add(4);
        buffer.LastChangedIndex.Should().Be(0);
        buffer.Add(5);
        buffer.LastChangedIndex.Should().Be(1);
        buffer.Add(6);
        buffer.LastChangedIndex.Should().Be(2);
        buffer.Add(7);
        buffer.LastChangedIndex.Should().Be(0);
        buffer.Add(8);
        buffer.LastChangedIndex.Should().Be(1);
        buffer.Clear();
        buffer.LastChangedIndex.Should().Be(-1);
    }

    [TestMethod]
    public void ShouldReverseOrderWithNegativeIndex()
    {
        CircularBuffer<int> buffer = new(6);
        buffer.AddRange(1, 2, 3, 4, 5, 6);
        buffer[-1].Should().Be(6);
        buffer[-2].Should().Be(5);
        buffer[-3].Should().Be(4);
        buffer[-4].Should().Be(3);
        buffer[-5].Should().Be(2);
        buffer[-6].Should().Be(1);
        buffer[-7].Should().Be(6);
        buffer[-8].Should().Be(5);
    }
}
