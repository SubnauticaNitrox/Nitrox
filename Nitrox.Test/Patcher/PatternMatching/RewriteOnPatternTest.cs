using HarmonyLib;
using NitroxModel.Helper;
using NitroxPatcher.PatternMatching;
using NitroxPatcher.PatternMatching.Ops;
using NitroxTest.Patcher;
using static System.Reflection.Emit.OpCodes;

namespace Nitrox.Test.Patcher.PatternMatching;

[TestClass]
public class RewriteOnPatternTest
{
    private static List<CodeInstruction> testCode;

    [TestInitialize]
    public void TestInitialize()
    {
        testCode =
        [
            new(Ldarg_1),
            new(Callvirt, Reflect.Property((ResolveEventArgs args) => args.Name).GetGetMethod()),
            new(Ldc_I4_S),
            new(Ldc_I4_0),
            new(Callvirt, Reflect.Method((string s) => s.Split(default))),
            new(Ldc_I4_0),
            new(Ldelem_Ref),
            new(Stloc_0)
        ];
    }

    [TestMethod]
    public void ShouldDoNothingWithEmptyInstructions()
    {
        Array.Empty<CodeInstruction>().RewriteOnPattern([]).Should().BeEmpty();
    }

    [TestMethod]
    public void ShouldReturnSameIfPatternDoesNotMatch()
    {
        testCode.RewriteOnPattern([Call], 0).Should().NotBeEmpty().And.HaveCount(testCode.Count);
    }

    [TestMethod]
    public void ShouldNotMatchIfPatternLargerThanIl()
    {
        testCode.RewriteOnPattern([..testCode]).Should().NotBeEmpty().And.HaveCount(testCode.Count);
        testCode.RewriteOnPattern([..testCode, Callvirt], 0).Should().NotBeEmpty().And.HaveCount(testCode.Count);
    }

    [TestMethod]
    public void ShouldNotMakeChangesIfNoOperationsInPattern()
    {
        CodeInstruction[] copy = testCode.Clone().ToArray();
        testCode.RewriteOnPattern([Ldc_I4_0], 2).Should().NotBeEmpty().And.HaveCount(testCode.Count);
        copy.ToPrettyString().Should().Be(testCode.ToPrettyString());
    }

    [TestMethod]
    public void ShouldThrowIfMatchingUnexpectedAmountOfTimes()
    {
        Assert.ThrowsException<Exception>(() => testCode.RewriteOnPattern([Ldc_I4_0], -1));
        Assert.ThrowsException<Exception>(() => testCode.RewriteOnPattern([Ldc_I4_0], 0));
        Assert.ThrowsException<Exception>(() => testCode.RewriteOnPattern([Ldc_I4_0], 1));
        Assert.ThrowsException<Exception>(() => testCode.RewriteOnPattern([Ldc_I4_0], 3));
    }

    [TestMethod]
    public void ShouldDifferIfOperationsExecuted()
    {
        CodeInstruction[] copy = testCode.Clone().ToArray();
        testCode.RewriteOnPattern([PatternOp.Change(Ldc_I4_0, i => i.opcode = Ldc_I4_1)], 2).Should().NotBeEmpty().And.HaveCount(testCode.Count);
        copy.ToPrettyString().Should().NotBe(testCode.ToPrettyString());

        // Pattern should now match without error.
        testCode.RewriteOnPattern([Ldc_I4_1, Callvirt]);
    }

    [TestMethod]
    public void ShouldNotInsertIfEmptyInsertOperation()
    {
        CodeInstruction[] copy = testCode.Clone().ToArray();
        testCode.RewriteOnPattern([Ldc_I4_0, []], 2).Should().NotBeEmpty().And.HaveCount(testCode.Count);
        copy.ToPrettyString().Should().Be(testCode.ToPrettyString());
    }

    [TestMethod]
    public void ShouldAddIlIfInsertOperationExecuted()
    {
        CodeInstruction[] copy = testCode.Clone().ToArray();
        int originalCount = copy.Length;
        testCode.RewriteOnPattern([Ldc_I4_0, [Ldc_I4_1]], 2).Should().NotBeEmpty().And.HaveCount(testCode.Count);
        copy.ToPrettyString().Should().NotBe(testCode.ToPrettyString());
        copy.Should().HaveCount(originalCount);
        testCode.Should().HaveCount(originalCount + 2);
    }

    [TestMethod]
    public void ShouldAddMultipleInstructionsIfInsertOperationHasMultiple()
    {
        CodeInstruction[] copy = testCode.Clone().ToArray();
        int originalCount = copy.Length;
        testCode.RewriteOnPattern([Ldc_I4_0, [Ldc_I4_1, Ldc_I4_1]], 2).Should().NotBeEmpty().And.HaveCount(testCode.Count);
        copy.ToPrettyString().Should().NotBe(testCode.ToPrettyString());
        copy.Should().HaveCount(originalCount);
        testCode.Should().HaveCount(originalCount + 4);

        // Pattern should now match without error.
        testCode.RewriteOnPattern([Ldc_I4_0, Ldc_I4_1, Ldc_I4_1], 2);
    }
}
