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
        throw new Exception("Test failed: Program continued to run after a fatal StatusCode call");
    }
    [TestMethod]
    public void ThrowStatusCodesLongException()
    {
        // Test if the text will wrap or if the messageBox will expand to accomodate for a long exception message
        DisplayStatusCode(StatusCode.onedriveFolderDetected, false, "This is a testing exception that is super long to test if text will wrap to fit the message box space, or potentially expand the message box as needed. Did you know that you are wasting your time reading this? Like actually there is nothing here you can stop reading, just run the test and you will see this text there. Alright this is probably long enough hopefully it works :) and btw Crabsnake is the best mod.");
    }

}
