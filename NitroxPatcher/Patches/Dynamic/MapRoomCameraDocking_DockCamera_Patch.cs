using System;
using System.CodeDom.Compiler;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.Helper;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class MapRoomCameraDocking_DockCamera_Patch : NitroxPatch, IDynamicPatch, INitroxPatch
{
	private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCameraDocking t) => t.DockCamera(null));

	public static void Postfix(MapRoomCameraDocking __instance, MapRoomCamera camera)
	{
		MapRoomCameras.EnsureCameraId(__instance, camera);
		NitroxPatch.Resolve<MapRoomCameras>().BroadcastDock(__instance, camera);
	}

	[GeneratedCode("Nitrox.Analyzers", "1.0.13.0")]
	public override void Patch(Harmony harmony)
	{
		PatchMultiple(harmony, TARGET_METHOD, null, new Action<MapRoomCameraDocking, MapRoomCamera>(Postfix).Method);
	}
}


