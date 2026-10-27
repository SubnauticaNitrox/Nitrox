using Nitrox.Model.Core;

namespace Nitrox.Model.Helper;

[TestClass]
public class StructEnumeratorTest
{
    [TestMethod]
    public void ShouldIterateToExactSize()
    {
        RentedArray<SessionId> rented = new(3);
        rented.Value[0] = (SessionId)1;
        rented.Value[1] = (SessionId)2;
        rented.Value[2] = (SessionId)3;
        StructEnumerator<SessionId, RentedArray<SessionId>> enumerator = new(rented.Value, 3, rented);
        int iterations = 0;
        // Code blow is the "Low-level C#" when using "foreach" statement. Can't use foreach because it calls a new "GetEnumerator" which will be a new value due to struct/value-based enumerator.
        try
        {
            while (enumerator.MoveNext())
            {
                ++iterations;
                SessionId sessionId = enumerator.Current;
                Console.WriteLine(sessionId);
            }
        }
        finally
        {
            enumerator.Dispose();
        }

        // Assert
        iterations.Should().Be(3);
        try
        {
            enumerator.MoveNext();
            Assert.Fail("Expected object to already be disposed");
        }
        catch (ObjectDisposedException)
        {
        }
    }

    /// <summary>
    ///     The underlying rented array gets disposed after first iteration. This test should confirm the object gets disposed.
    /// </summary>
    [TestMethod]
    public void ShouldThrowIfIteratedTwice()
    {
        RentedArray<SessionId> rented = new(3);
        rented.Value[0] = (SessionId)1;
        rented.Value[1] = (SessionId)2;
        rented.Value[2] = (SessionId)3;
        StructEnumerator<SessionId, RentedArray<SessionId>> enumerator = new(rented.Value, 3, rented);
        foreach (SessionId sessionId in enumerator)
        {
            Console.WriteLine(sessionId);
        }

        // Assert
        try
        {
            foreach (SessionId sessionId in enumerator)
            {
                Console.WriteLine(sessionId);
            }
            Assert.Fail("Expected object to already be disposed");
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
