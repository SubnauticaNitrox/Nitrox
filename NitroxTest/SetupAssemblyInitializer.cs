using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;

namespace NitroxTest
{
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            NitroxEnvironment.Set(NitroxEnvironment.Types.TESTING);
        }
    }
}
