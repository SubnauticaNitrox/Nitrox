global using Nitrox.Test.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace Nitrox.Test;

[TestClass]
public class SetupAssemblyInitializer
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        NitroxEnvironment.Set(NitroxEnvironment.Types.TESTING);
        Log.Setup();
    }
}
