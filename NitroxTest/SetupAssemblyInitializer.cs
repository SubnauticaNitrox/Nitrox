using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxTest
{
    [TestClass]
    public static class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            NitroxEnvironment.Set(NitroxEnvironmentTypes.TESTING);
            Log.Setup();
        }
    }
}
