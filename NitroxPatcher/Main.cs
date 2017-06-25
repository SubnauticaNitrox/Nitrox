using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;

namespace NitroxPatcher
{
    class Main
    {
        public static void Execute()
        {
            Console.WriteLine("Patching subnautica for nitrox");
            HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony.1");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Console.WriteLine("Completed patching for nitrox using " + Assembly.GetExecutingAssembly().FullName);
        }
    }
}
