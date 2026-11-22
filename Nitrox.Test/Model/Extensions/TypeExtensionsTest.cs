using Nitrox.Test;

namespace Nitrox.Model.Extensions;

[TestClass]
public class TypeExtensionsTest
{
    [TestMethod]
    public void ShouldReturnTypeCodePath()
    {
        typeof(SetupAssemblyInitializer).GetCsFilePathFromType().Should().BeEquivalentTo("Nitrox.Test/SetupAssemblyInitializer.cs");
        Assert.Throws<Exception>(() => typeof(TypeExtensionsTest).GetCsFilePathFromType());
        typeof(TypeExtensions).GetCsFilePathFromType().Should().BeEquivalentTo("Nitrox.Model/Extensions/TypeExtensions.cs");
    }
}
