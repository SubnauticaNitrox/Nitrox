using System;
using System.Collections.Generic;
using Harmony;
using System.Reflection;
using NitroxPatcher.Patches;

namespace NitroxPatcher
{
    class Main
    {
        private static readonly List<NitroxPatch> patches = new List<NitroxPatch>()
        {
            new ArmsController_Update_Patch(),
            new ArmsController_Start_Patch(),
            new BuilderPatch(),
            new ClipMapManager_HideEntities_Patch(),
            new ClipMapManager_ShowEntities_Patch(),
            new Constructable_Construct_Patch(),
            new Equipment_AddItem_Patch(),
            new Equipment_RemoveItem_Patch(),
            new Pickupable_Pickup_Patch(),
            new Pickupable_Drop_Patch(),
            new SpawnConsoleCommand_Patch(),
            new ConstructorInput_Craft_Patch(),
            new ConstructorInput_OnCraftingBegin_Patch(),
            new Constructable_Construct_Patch(),
            new MedicalCabinet_OnHandClick_Patch(),
            new BaseGhost_Finish_Patch(),
            new uGUI_MainMenu_Start_Patch()
        };

        public static void Execute()
        {
            Console.WriteLine("Patching subnautica for nitrox");
            // Enabling this creates a log file on your desktop (why there?), showing the emitted IL instructions.
            HarmonyInstance.DEBUG = false;

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
