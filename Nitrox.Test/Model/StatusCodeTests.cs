using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Forms;
using static NitroxModel.DisplayStatusCodes;
using System.Diagnostics;
namespace NitroxModel;

[TestClass]
public class StatusCodeTests
{
    // Vars for counting repeat codes to check if they are infinitely repeating and therefore fatal
    private static int repeatCodeCountDisplay = 0;
    private static StatusCode lastCodeDisplay = StatusCode.SUCCESS;
    private static int repeatCodeCountPrint = 0;
    private static StatusCode lastCodePrint = StatusCode.SUCCESS;
    public static bool TestFatalDisplayStatusCode(StatusCode statusCode, bool fatal, string exception)
    {
        // If the statusCode is the same as the last one, increase the repeat code count
        if (statusCode == lastCodeDisplay && statusCode != StatusCode.CONNECTION_FAIL_CLIENT)
        {
            repeatCodeCountDisplay++;
        }
        else
        {
            repeatCodeCountDisplay = 0;
        }
        // If there have been 3 of the same code in a row, including this one, then the error is fatal
        if (repeatCodeCountDisplay == 2)
        {
            fatal = true;
        }
        // Set the last code variable
        lastCodeDisplay = statusCode;
        // Display a popup message box using CustomMessageBox.cs which has most of the buttons and strings filled in with a placeholder for the statusCode
        CustomMessageBox customMessage = new(statusCode, exception);
        customMessage.StartPosition = FormStartPosition.CenterParent;
        customMessage.ShowDialog();
        // If the error is fatal,
        if (fatal)
        {
            // Normally we would exit nitrox, but we return true for testing
            return true;
        }
        // If the error is not fatal, we would continue running but we return false for testing
        return false;
    }

    // Print the statusCode to the server console(only for statusCodes that are due to a server-side crash)
    public static bool TestFatalPrintStatusCode(StatusCode statusCode, bool fatal, string exception)
    {
        // If the code is the same as the one we last printed, increment the repeated codes counter
        if (statusCode == lastCodePrint && statusCode != StatusCode.CONNECTION_FAIL_CLIENT)
        {
            repeatCodeCountPrint++;
        }
        // If the code is different, reset the counter
        else
        {
            repeatCodeCountPrint = 0;
        }
        // If the same code happens 3 times in a row, then the error is fatal
        if (repeatCodeCountPrint == 2)
        {
            fatal = true;
        }
        // Set the last StatusCode variable
        lastCodePrint = statusCode;
        // Log the statusCode message to console
        Debug.WriteLine(string.Concat("Status code = ", statusCode.ToString("D"), " <- Look up this code on the nitrox website for more information about this error." + "Exception message: " + exception));
        // If the error is fatal,
        if (fatal)
        {
            // Normally we would exit nitrox, but we return true for testing
            return true;
        }
        // If the error is not fatal, we would continue running but we return false for testing purposes
        return false;
    }


    [TestMethod]
    public void ThrowStatusCodesNonFatalDisplay()
    {
        var statusCodeNames = Enum.GetNames(typeof(StatusCode));
        // Display one of every status code once non-fatally, should display all of them once and show "test passed"
        for (int i = 0; i < statusCodeNames.Length; i++)
        {
            DisplayStatusCode((StatusCode)Enum.Parse(typeof(StatusCode), statusCodeNames[i]), false, statusCodeNames[i] + " Testing exception");
        }
    }
    [TestMethod]
    public void ThrowStatusCodesFatalDisplay()
    {
        // Display one of every status code once fatally, should only display the first one and then exit, showing "Test aborted"
        if (!TestFatalDisplayStatusCode(StatusCode.DEAD_PIRATES_TELL_NO_TALES, true, StatusCode.DEAD_PIRATES_TELL_NO_TALES.ToString() + " Testing exception"))
        {
            throw new Exception("Test failed: Program continued to run after a fatal StatusCode call");
        }

    }
    [TestMethod]
    public void ThrowStatusCodesLongExceptionDisplay()
    {

        // Test if the text will wrap or if the messageBox will expand to accomodate for a long exception message
        DisplayStatusCode(StatusCode.FILE_SYSTEM_ERR, false, "This is a testing exception that is super long to test if text will wrap to fit the message box space, or potentially expand the message box as needed. Did you know that you are wasting your time reading this? Like actually there is nothing here you can stop reading, just run the test and you will see this text there. Alright this is probably long enough hopefully it works :) and btw Crabsnake is the best mod.");
    }
    [TestMethod]
    public void InfinitelyThrowSameCodeDisplay()
    { // Test if after throwing the same message 3 times, if it will exit fatally
        TestFatalDisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, false, " Infinite loop testing exception");
        TestFatalDisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, false, " Infinite loop testing exception");
        if (!TestFatalDisplayStatusCode(StatusCode.INVALID_VARIABLE_VAL, false, " Infinite loop testing exception"))
        {
            throw new Exception("Test failed: program did not exit after 3 StatusCode exception calls for the same error");
        }
    }
    [TestMethod]
    public void InfinitelyThrowSameCodePrint()
    {
        TestFatalPrintStatusCode(StatusCode.INVALID_VARIABLE_VAL, false, " Infinite loop testing exception");
        TestFatalPrintStatusCode(StatusCode.INVALID_VARIABLE_VAL, false, " Infinite loop testing exception");
        if (!TestFatalPrintStatusCode(StatusCode.INVALID_VARIABLE_VAL, false, " Infinite loop testing exception"))
        {
            throw new Exception("Test failed: program did not exit after 3 StatusCode exception calls for the same error");
        }
    }
    [TestMethod]
    public void ThrowStatusCodesNonFatalPrint()
    {
        // Display one of every status code once non-fatally, should display all of them once and show "test passed"
        var statusCodeNames = Enum.GetNames(typeof(StatusCode));
        for (int i = 0; i < statusCodeNames.Length; i++)
        {
            PrintStatusCode((StatusCode)Enum.Parse(typeof(StatusCode), statusCodeNames[i]), false, statusCodeNames[i] + " Testing exception");
        }
    }
    [TestMethod]
    public void ThrowStatusCodesFatalPrint()
    {
        // Display one of every status code once fatally, should only display the first one
        if (!TestFatalPrintStatusCode(StatusCode.DEAD_PIRATES_TELL_NO_TALES, true, StatusCode.DEAD_PIRATES_TELL_NO_TALES.ToString() + "Testing exception"))
        {
            throw new Exception("Test failed: Program continued to run after a fatal StatusCode call");
        }
    }
    [TestMethod]
    public void ThrowStatusCodesLongExceptionPrint()
    {
        // Test if the text will wrap or if the messageBox will expand to accomodate for a long exception message
        PrintStatusCode(StatusCode.FILE_SYSTEM_ERR, false, "This is a testing exception that is super long to test if text will wrap to fit the message box space, or potentially expand the message box as needed. Did you know that you are wasting your time reading this? Like actually there is nothing here you can stop reading, just run the test and you will see this text there. Alright this is probably long enough hopefully it works :) and btw Crabsnake is the best mod.");
    }
}
