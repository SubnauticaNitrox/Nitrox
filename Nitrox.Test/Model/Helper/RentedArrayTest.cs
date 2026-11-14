namespace Nitrox.Model.Helper;

[TestClass]
public class RentedArrayTest
{
    [TestMethod]
    public void ShouldIterateToExactSizeOnly()
    {
        using RentedArray<int> test = new(10);
        test.Value.AsSpan().Fill(163);
        int count = 0;
        foreach (int i in test)
        {
            i.Should().Be(163);
            count++;
        }
        count.Should().Be(10);
    }
}
