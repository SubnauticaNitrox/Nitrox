using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Nitrox.Test.Model.Platforms;

/// <summary>
///     Test method attribute, that will only run the test on the specified platform.
/// </summary>
/// <param name="platform">case-insensitive platform, i.e: linux, windows, osx</param>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class OSTestMethodAttribute(
    OperatingSystems platform,
    [CallerFilePath] string callerFilePath = "",
    [CallerLineNumber] int callerLineNumber = -1
) : TestMethodAttribute(callerFilePath, callerLineNumber)
{
    private readonly OperatingSystems platform = platform;

    public override Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        if (!OperatingSystem.IsOSPlatform(GetPlatformString()))
        {
            return Task.FromResult(new TestResult[]
            {
                new TestResult
                {
                    Outcome = UnitTestOutcome.Inconclusive,
                    TestContextMessages = $"This test can only be run on {GetPlatformString()}"
                }
            });
        }
        return base.ExecuteAsync(testMethod);
    }

    private string GetPlatformString() =>
        platform switch
        {
            OperatingSystems.Windows => "Windows",
            OperatingSystems.Linux => "Linux",
            OperatingSystems.OSX => "OSX",
            OperatingSystems.FreeBSD => "FreeBSD",
            _ => throw new InvalidOperationException($"Unknown platform {(int)platform}")
        };
}
