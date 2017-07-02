using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using NitroxPatcher.Patches;
using NitroxModel.Helper;

namespace NitroxPatcher
{
    class Main
    {
        public static void Execute()
        {
            Console.WriteLine("Patching subnautica for nitrox");
            HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
            ClipMapManager_ShowEntities_Patch.Patch(harmony);
            ClipMapManager_HideEntities_Patch.Patch(harmony);

            Console.WriteLine("Completed patching for nitrox using " + Assembly.GetExecutingAssembly().FullName);
        }
    }
}
