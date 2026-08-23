global using Nitrox.Test.Helper;
using Nitrox.Model.Core;
using Nitrox.Model.Logger;

namespace Nitrox.Test;

[TestClass]
public static class SetupAssemblyInitializer
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        NitroxEnvironment.Set(NitroxEnvironment.Types.TESTING);
        Log.Setup();
    }
}
