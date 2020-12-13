using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Model.Core;
using Nitrox.Model.Logger;

namespace Nitrox.Test
{
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
}
