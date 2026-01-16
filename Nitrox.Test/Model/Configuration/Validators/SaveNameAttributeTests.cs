namespace Nitrox.Model.Configuration.Validators;

[TestClass]
public class SaveNameAttributeTests
{
    private SaveNameAttribute attr = null!;

    [TestInitialize]
    public void Setup()
    {
        attr = new();
    }

    [TestMethod]
    public void ShouldValidateSaveName()
    {
        attr.IsValid("Hello").Should().BeTrue();
        attr.IsValid("").Should().BeTrue();

        attr.IsValid(null!).Should().BeFalse();
        attr.IsValid("/").Should().BeFalse();
        attr.IsValid("Hello.").Should().BeFalse();
    }
}
