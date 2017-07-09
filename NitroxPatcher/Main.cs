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
        private static readonly List<NitroxPatch> patches = new List<NitroxPatch>()
        {
            new BuilderPatch(),
            new ClipMapManager_HideEntities_Patch(),
            new ClipMapManager_ShowEntities_Patch(),
            new Constructable_Construct_Patch(),
            new Pickupable_Pickup_Patch(),
            new SpawnConsoleCommand_Patch(),
            new ConstructorInput_Craft()
        };

        public static void Execute()
        {
            Console.WriteLine("Patching subnautica for nitrox");
            HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");

            foreach(NitroxPatch patch in patches)
            {
                Console.WriteLine("[NITROX] Applying " + patch.GetType());
                patch.Patch(harmony);
            }


            Console.WriteLine("Completed patching for nitrox using " + Assembly.GetExecutingAssembly().FullName);
        }
    }
}
