global using Nitrox.Test.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;

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
