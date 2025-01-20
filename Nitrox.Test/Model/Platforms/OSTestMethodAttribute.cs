namespace Nitrox.Test.Model.Platforms;

[AttributeUsage(AttributeTargets.Method)]
public class OSTestMethodAttribute : TestMethodAttribute
{
    public string Platform { get;}

    /// <summary>
    /// Test method attribute, that will only run the test on the specified platform.
    /// </summary>
    /// <param name="platform">case insensitive platform, i.e: linux, windows, osx</param>
    public OSTestMethodAttribute(string platform)
    {
        Platform = platform;
    }

    public override TestResult[] Execute(ITestMethod testMethod)
    {
        if (!OperatingSystem.IsOSPlatform(Platform))
        {
            return [
                new TestResult()
                {
                    Outcome = UnitTestOutcome.Inconclusive,
                    TestContextMessages = $"This test can only be run on {Platform}"
                }
            ];
        }

        return base.Execute(testMethod);
    }
}
