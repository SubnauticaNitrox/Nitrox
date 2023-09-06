using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;
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

        string currentAsmFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        if (Directory.Exists(Path.Combine(currentAsmFolder, "LanguageFiles")))
        {
            NitroxUser.launcherPath = currentAsmFolder;
        }
    }
}
