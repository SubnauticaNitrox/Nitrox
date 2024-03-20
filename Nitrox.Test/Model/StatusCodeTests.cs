using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Forms;
using static NitroxModel.DisplayStatusCodes;
using System.Diagnostics;
namespace NitroxModel;

[TestClass]
public class StatusCodeTests
{
    [TestMethod]
    public void ThrowStatusCodesDisplay()
    {
        var statusCodeNames = Enum.GetNames(typeof(StatusCode));
        // Display one of every status code once non-fatally, should display all of the popup ones once, and the ingame log ones will be silent, then show "test passed"
        for (int i = 0; i < statusCodeNames.Length; i++)
        {
            DisplayStatusCode((StatusCode)Enum.Parse(typeof(StatusCode), statusCodeNames[i]), statusCodeNames[i] + " Testing exception");
        }
    }
    [TestMethod]
    public void ThrowStatusCodeLongExceptionDisplay()
    {

        // Test if the text will wrap or if the messageBox will expand to accomodate for a long exception message
        DisplayStatusCode(StatusCode.FILE_SYSTEM_ERR, "This is a testing exception that is super long to test if text will wrap to fit the message box space, or potentially expand the message box as needed. Did you know that you are wasting your time reading this? Like actually there is nothing here you can stop reading, just run the test and you will see this text there. Alright this is probably long enough hopefully it works :) and btw Crabsnake is the best mod.");
    }
    [TestMethod]
    public void ThrowStatusCodesPrint()
    {
        // Display one of every status code once non-fatally, should print them all which will be visible in the results and show "test passed"
        var statusCodeNames = Enum.GetNames(typeof(StatusCode));
        for (int i = 0; i < statusCodeNames.Length; i++)
        {
            PrintStatusCode((StatusCode)Enum.Parse(typeof(StatusCode), statusCodeNames[i]), statusCodeNames[i] + " Testing exception");
        }
    }
    [TestMethod]
    public void ThrowStatusCodesLongExceptionPrint()
    {
        // Test if the console print can accomodate a long exception message
        PrintStatusCode(StatusCode.FILE_SYSTEM_ERR, "This is a testing exception that is super long to test if text will wrap to fit the message box space, or potentially expand the message box as needed. Did you know that you are wasting your time reading this? Like actually there is nothing here you can stop reading, just run the test and you will see this text there. Alright this is probably long enough hopefully it works :) and btw Crabsnake is the best mod.");
    }
}
