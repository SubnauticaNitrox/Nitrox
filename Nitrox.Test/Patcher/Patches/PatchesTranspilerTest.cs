using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient;
using NitroxClient.Patching.Patches;
using NitroxClient.Patching.Patches.Dynamic;
using NitroxClient.Patching.PatternMatching;
using NitroxPatcher.Patches;
using NitroxPatcher.Patches.Dynamic;
using NitroxPatcher.Patches.Persistent;
using NitroxTest.Patcher;

namespace Nitrox.Test.Patcher.Patches;

[TestClass]
public class PatchesTranspilerTest
{
    // Add "true" to any of those elements to have its transformed IL printed.
    public static IEnumerable<object[]> TranspilerPatchClasses =>
    [
        [typeof(NitroxClient.Patching.Patches.Dynamic.AggressiveWhenSeeTarget_ScanForAggressionTarget_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.AttackCyclops_OnCollisionEnter_Patch), -17],
        [typeof(NitroxClient.Patching.Patches.Dynamic.AttackCyclops_UpdateAggression_Patch), -23],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Bullet_Update_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.BaseDeconstructable_Deconstruct_Patch), NitroxClient.Patching.Patches.Dynamic.BaseDeconstructable_Deconstruct_Patch.InstructionsToAdd(true).Count() * 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.BaseHullStrength_CrushDamageUpdate_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.BreakableResource_SpawnResourceFromPrefab_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Builder_TryPlace_Patch), NitroxClient.Patching.Patches.Dynamic.Builder_TryPlace_Patch.InstructionsToAdd1.Count + NitroxClient.Patching.Patches.Dynamic.Builder_TryPlace_Patch.InstructionsToAdd2.Count],
        [typeof(NitroxClient.Patching.Patches.Persistent.CellManager_TryLoadCacheBatchCells_Patch), 4],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Charger_Update_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CoffeeVendingMachine_OnMachineUse_Patch), 6],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Constructable_Construct_Patch), NitroxClient.Patching.Patches.Dynamic.Constructable_Construct_Patch.InstructionsToAdd.Count],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Constructable_DeconstructAsync_Patch), NitroxClient.Patching.Patches.Dynamic.Constructable_DeconstructAsync_Patch.InstructionsToAdd.Count],
        [typeof(NitroxClient.Patching.Patches.Dynamic.ConstructableBase_SetState_Patch), NitroxClient.Patching.Patches.Dynamic.ConstructableBase_SetState_Patch.InstructionsToAdd.Count],
        [typeof(NitroxClient.Patching.Patches.Dynamic.ConstructorInput_OnCraftingBegin_Patch), 7],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CrashHome_Spawn_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CrashHome_Update_Patch), -5],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CreatureDeath_OnKillAsync_Patch), 5],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CreatureDeath_SpawnRespawner_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CyclopsDestructionEvent_DestroyCyclops_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CyclopsDestructionEvent_SpawnLootAsync_Patch), 7],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CyclopsHelmHUDManager_Update_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CyclopsShieldButton_OnClick_Patch), -6],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CyclopsSonarButton_Update_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.CyclopsSonarDisplay_NewEntityOnSonar_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.DevConsole_Update_Patch), 0],
        [typeof(Drillable_ManagedUpdate_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Drillable_SpawnLootAsync_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Eatable_IterateDespawn_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.EndCreditsManager_OnLateUpdate_Patch), 1],
        [typeof(NitroxClient.Patching.Patches.Dynamic.EnergyMixin_SpawnDefaultAsync_Patch), -64],
        [typeof(NitroxClient.Patching.Patches.Dynamic.EntityCell_AwakeAsync_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.EntityCell_SleepAsync_Patch), 2],
        [typeof(ErrorMessage_OnLateUpdate_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.EscapePod_Start_Patch), 43],
        [typeof(NitroxClient.Patching.Patches.Dynamic.FireExtinguisherHolder_TakeTankAsync_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.FireExtinguisherHolder_TryStoreTank_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Flare_Update_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.FootstepSounds_OnStep_Patch), 6],
        [typeof(NitroxClient.Patching.Patches.Dynamic.GrowingPlant_SpawnGrownModelAsync_Patch), -1],
        [typeof(NitroxClient.Patching.Patches.Persistent.GameInputSystem_Initialize_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Player_TriggerInfectionRevealAsync_Patch), 1],
        [typeof(NitroxClient.Patching.Patches.Dynamic.IngameMenu_OnSelect_Patch), -2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.IngameMenu_QuitGameAsync_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.IngameMenu_QuitSubscreen_Patch), -24],
        [typeof(NitroxClient.Patching.Patches.Dynamic.ItemsContainer_DestroyItem_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Knife_OnToolUseAnim_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.LargeRoomWaterPark_OnDeconstructionStart_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.LargeWorldEntity_UpdateCell_Patch), 1],
        [typeof(NitroxClient.Patching.Patches.Dynamic.LaunchRocket_OnHandClick_Patch), -8],
        [typeof(NitroxClient.Patching.Patches.Dynamic.LeakingRadiation_Update_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Persistent.MainGameController_StartGame_Patch), 1],
        [typeof(NitroxClient.Patching.Patches.Dynamic.MeleeAttack_CanDealDamageTo_Patch), 4],
        [typeof(NitroxClient.Patching.Patches.Dynamic.PDAScanner_Scan_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.PickPrefab_AddToContainerAsync_Patch), 4],
        [typeof(NitroxClient.Patching.Patches.Dynamic.PipeSurfaceFloater_FixedUpdate_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Player_OnKill_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.PrecursorDoorMotorModeSetter_OnTriggerEnter_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.PrecursorMoonPoolTrigger_Update_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Respawn_Start_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.RocketConstructor_StartRocketConstruction_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SpawnConsoleCommand_SpawnAsync_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SpawnOnKill_OnKill_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SubConsoleCommand_OnConsoleCommand_sub_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.ToggleLights_SetLightsActive_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Trashcan_Update_Patch), 4],
        [typeof(NitroxClient.Patching.Patches.Persistent.uGUI_OptionsPanel_AddAccessibilityTab_Patch), -10],
        [typeof(NitroxClient.Patching.Patches.Dynamic.uGUI_PDA_Initialize_Patch), 2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.uGUI_PDA_SetTabs_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.uGUI_Pings_IsVisibleNow_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.uGUI_SceneIntro_HandleInput_Patch), -2],
        [typeof(NitroxClient.Patching.Patches.Dynamic.uGUI_SceneIntro_IntroSequence_Patch), 8],
        [typeof(NitroxClient.Patching.Patches.Dynamic.uSkyManager_SetVaryingMaterialProperties_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Welder_Weld_Patch), 1],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Poop_Perform_Patch), 1],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeaDragonMeleeAttack_OnTouchFront_Patch), 9],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeaDragonMeleeAttack_SwatAttack_Patch), 4],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeaTreaderSounds_SpawnChunks_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.Vehicle_TorpedoShot_Patch), 3],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeamothTorpedo_Update_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeaTreader_UpdatePath_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeaTreader_UpdateTurning_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.SeaTreader_Update_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.StasisSphere_LateUpdate_Patch), 0],
        [typeof(NitroxClient.Patching.Patches.Dynamic.WaterParkCreature_BornAsync_Patch), 6],
        [typeof(NitroxClient.Patching.Patches.Dynamic.WaterParkCreature_ManagedUpdate_Patch), 2],
    ];

    [TestMethod]
    public void AllTranspilerPatchesHaveSanityTest()
    {
        Type[] allPatchesWithTranspiler = typeof(Main).Assembly.GetTypes().Where(p => typeof(NitroxPatch).IsAssignableFrom(p) && p.IsClass).Where(x => x.GetMethod("Transpiler") != null).ToArray();

        foreach (Type patch in allPatchesWithTranspiler)
        {
            if (TranspilerPatchClasses.All(x => (Type)x[0] != patch))
            {
                Assert.Fail($"{patch.Name} has an Transpiler but is not included in the test-suit and {nameof(TranspilerPatchClasses)}.");
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(TranspilerPatchClasses))]
    public void AllPatchesTranspilerSanity(Type patchClassType, int ilDifference, bool logInstructions = false)
    {
        MethodInfo transpilerMethod = patchClassType.GetMethod("Transpiler");
        if (transpilerMethod == null)
        {
            Assert.Fail($"Could not find a \"Transpiler\" method inside {patchClassType.Name}");
        }

        FieldInfo[] targetMethodInfos = patchClassType.GetRuntimeFields()
                                                      .Where(x => x.Name.Replace("_", "").StartsWith("targetMethod", StringComparison.OrdinalIgnoreCase))
                                                      .OrderBy(fieldInfo => fieldInfo.Name)
                                                      .ToArray();

        if (targetMethodInfos.Length == 0)
        {
            Assert.Fail($"Could not find a \"TARGET_METHOD\" or \"targetMethod\" field inside {patchClassType.Name}");
        }

        foreach (FieldInfo field in targetMethodInfos)
        {
            MethodInfo targetMethod = field.GetValue(null) as MethodInfo;
            TestTranspilerMethod(patchClassType, targetMethod, transpilerMethod, ilDifference, logInstructions);
        }
    }

    private static void TestTranspilerMethod(Type patchClassType, MethodInfo targetMethod, MethodInfo transpilerMethod, int ilDifference, bool logInstructions = false)
    {
        List<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(targetMethod).ToList();
        List<CodeInstruction> originalIlCopy = PatchTestHelper.GetInstructionsFromMethod(targetMethod).ToList(); // Our custom pattern matching replaces OpCode/Operand in place, therefor we need a copy to compare if changes are present

        List<object> injectionParameters = [];
        foreach (ParameterInfo parameterInfo in transpilerMethod.GetParameters())
        {
            if (parameterInfo.ParameterType == typeof(MethodBase))
            {
                injectionParameters.Add(targetMethod);
            }
            else if (parameterInfo.ParameterType == typeof(IEnumerable<CodeInstruction>))
            {
                injectionParameters.Add(originalIl);
            }
            else if (parameterInfo.ParameterType == typeof(ILGenerator))
            {
                injectionParameters.Add(GetILGenerator(targetMethod, patchClassType));
            }
            else
            {
                Assert.Fail($"Unexpected parameter type: {parameterInfo.ParameterType} inside Transpiler method of {patchClassType.Name}");
            }
        }

        List<CodeInstruction> transformedIl = (transpilerMethod.Invoke(null, injectionParameters.ToArray()) as IEnumerable<CodeInstruction>)?.ToList();

        if (logInstructions)
        {
            Console.WriteLine(transformedIl.ToPrettyString());
        }

        if (transformedIl == null || transformedIl.Count == 0)
        {
            Assert.Fail($"Calling {patchClassType.Name}.Transpiler() through reflection returned null or an empty list.");
        }

        originalIlCopy.Count.Should().Be(transformedIl.Count - ilDifference);
        Assert.IsFalse(originalIlCopy.SequenceEqual(transformedIl, new CodeInstructionComparer()), $"The transpiler patch of {patchClassType.Name} did not change the IL");
    }

    private static readonly ModuleBuilder patchTestModule;

    static PatchesTranspilerTest()
    {
        AssemblyName asmName = new();
        asmName.Name = "PatchTestAssembly";

        PersistedAssemblyBuilder myAsmBuilder = new(asmName, typeof(object).Assembly);
        patchTestModule = myAsmBuilder.DefineDynamicModule(asmName.Name);
    }

    /// This complicated generation is required for ILGenerator.DeclareLocal to work
    private static ILGenerator GetILGenerator(MethodInfo method, Type generatingType)
    {
        TypeBuilder myTypeBld = patchTestModule.DefineType($"{generatingType}_PatchTestType", TypeAttributes.Public);

        return myTypeBld.DefineMethod(method.Name, MethodAttributes.Public, method.ReturnType, method.GetParameters().Types()).GetILGenerator();
    }
}

public class CodeInstructionComparer : IEqualityComparer<CodeInstruction>
{
    public bool Equals(CodeInstruction x, CodeInstruction y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null)
        {
            return false;
        }
        if (y is null)
        {
            return false;
        }
        if (x.GetType() != y.GetType())
        {
            return false;
        }
        return x.opcode.Equals(y.opcode) && Equals(x.operand, y.operand);
    }

    public int GetHashCode(CodeInstruction obj)
    {
        unchecked
        {
            return (obj.opcode.GetHashCode() * 397) ^ (obj.operand != null ? obj.operand.GetHashCode() : 0);
        }
    }
}
