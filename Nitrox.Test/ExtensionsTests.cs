using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel;

[TestClass]
public class ExtensionsTest
{
    [TestMethod]
    public void RemoveAllFast_ShouldDoNothingWhenEmptyList()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (_, _) => false);
        list.Should().BeEquivalentTo("one", "two", "three", "four");
    }

    [TestMethod]
    public void RemoveAllFast_ShouldDoNothingWhenAlwaysFalsePredicate()
    {
        object[] list = new object[0];
        list.RemoveAllFast((object)null, static (_, _) => true);
        list.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveAllFast_ThrowsErrorIfFixedSizeList()
    {
        string[] list = { "one", "two", "three" };
        Assert.ThrowsException<NotSupportedException>(() => list.RemoveAllFast((object)null, static (item, _) => item == "one"));
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveFirstItem()
    {
        List<string> list = new() { "one", "two", "three" };
        list.RemoveAllFast((object)null, static (item, _) => item == "one");
        list.Should().BeEquivalentTo("two", "three");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveMidItems()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (item, _) => item == "two" || item == "three");
        list.Should().BeEquivalentTo("one", "four");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveEndItem()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (item, _) => item == "four");
        list.Should().BeEquivalentTo("one", "two", "three");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveAllItems()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (_, _) => true);
        list.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveItemsWithExtraParameterInPredicate()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast(3, static (item, length) => item.Length == length);
        list.Should().BeEquivalentTo("three", "four");
    }
}
