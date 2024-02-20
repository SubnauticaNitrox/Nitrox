using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel;

[TestClass]
public class StatusCodeTests
{
    [TestMethod]
    public void ThrowStatusCodesNonFatal()
    {
        // Display one of every status code once non-fatally, should display all of them once and show "test passed"
        for (int i = 0; i < Enum.GetNames(typeof(StatusCode)).Length; i++)
        {
            DisplayStatusCode((StatusCode)i, false, "Testing exception");
        }
    }
    [TestMethod]
    public void ThrowStatusCodesFatal() // Should show up as "aborted" after running it in VS
    {
        // Display one of every status code once fatally, should only display the first one
        for (int i = 0; i < Enum.GetNames(typeof(StatusCode)).Length; i++)
        {
            DisplayStatusCode((StatusCode)i, true, "Testing exception");
        }
        throw new Exception("Test failed: Program continued after a fatal StatusCode call");
    }
}
