using System.Reflection.Emit;
using HarmonyLib;
using NitroxPatcher.Patches;
using NitroxPatcher.Patches.Dynamic;
using NitroxPatcher.Patches.Persistent;
using NitroxPatcher.PatternMatching;
using NitroxTest.Patcher;

namespace Nitrox.Test.Patcher.Patches;

[TestClass]
public class PatchesTranspilerTest
{
    // Add "true" to any of those elements to have its transformed IL printed.
    public static IEnumerable<object[]> TranspilerPatchClasses =>
    [
        [typeof(AggressiveWhenSeeTarget_ScanForAggressionTarget_Patch), 3],
        [typeof(AttackCyclops_OnCollisionEnter_Patch), -17],
        [typeof(AttackCyclops_UpdateAggression_Patch), -23],
        [typeof(Bullet_Update_Patch), 3],
        [typeof(BaseDeconstructable_Deconstruct_Patch), BaseDeconstructable_Deconstruct_Patch.InstructionsToAdd(true).Count() * 2],
        [typeof(BaseHullStrength_CrushDamageUpdate_Patch), 3],
        [typeof(BreakableResource_SpawnResourceFromPrefab_Patch), 2],
        [typeof(Builder_TryPlace_Patch), Builder_TryPlace_Patch.InstructionsToAdd1.Count + Builder_TryPlace_Patch.InstructionsToAdd2.Count],
        [typeof(CellManager_TryLoadCacheBatchCells_Patch), 4],
        [typeof(Constructable_Construct_Patch), Constructable_Construct_Patch.InstructionsToAdd.Count],
        [typeof(Constructable_DeconstructAsync_Patch), Constructable_DeconstructAsync_Patch.InstructionsToAdd.Count],
        [typeof(ConstructableBase_SetState_Patch), ConstructableBase_SetState_Patch.InstructionsToAdd.Count],
        [typeof(ConstructorInput_OnCraftingBegin_Patch), 7],
        [typeof(CrafterLogic_TryPickupSingleAsync_Patch), 4],
        [typeof(CrashHome_Spawn_Patch), 2],
        [typeof(CrashHome_Update_Patch), -5],
        [typeof(CreatureDeath_OnKillAsync_Patch), 9],
        [typeof(CreatureDeath_SpawnRespawner_Patch), 2],
        [typeof(CyclopsDestructionEvent_DestroyCyclops_Patch), 3],
        [typeof(CyclopsDestructionEvent_SpawnLootAsync_Patch), 7],
        [typeof(CyclopsShieldButton_OnClick_Patch), -6],
        [typeof(CyclopsSonarButton_Update_Patch), 3],
        [typeof(CyclopsSonarDisplay_NewEntityOnSonar_Patch), 3],
        [typeof(DevConsole_Update_Patch), 0],
        [typeof(Eatable_IterateDespawn_Patch), 2],
        [typeof(EnergyMixin_SpawnDefaultAsync_Patch), -64],
        [typeof(EntityCell_AwakeAsync_Patch), 2],
        [typeof(EntityCell_SleepAsync_Patch), 2],
        [typeof(Equipment_RemoveItem_Patch), 7],
        [typeof(EscapePod_Start_Patch), 43],
        [typeof(FireExtinguisherHolder_TakeTankAsync_Patch), 2],
        [typeof(FireExtinguisherHolder_TryStoreTank_Patch), 3],
        [typeof(Flare_Update_Patch), 0],
        [typeof(FootstepSounds_OnStep_Patch), 6],
        [typeof(GameInput_Initialize_Patch), 5],
        [typeof(GrowingPlant_SpawnGrownModelAsync_Patch), -1],
        [typeof(Player_TriggerInfectionRevealAsync_Patch), 1],
        [typeof(IngameMenu_OnSelect_Patch), -2],
        [typeof(IngameMenu_QuitGameAsync_Patch), 2],
        [typeof(IngameMenu_QuitSubscreen_Patch), -24],
        [typeof(ItemsContainer_DestroyItem_Patch), 2],
        [typeof(Knife_OnToolUseAnim_Patch), 0],
        [typeof(LargeRoomWaterPark_OnDeconstructionStart_Patch), 3],
        [typeof(LargeWorldEntity_UpdateCell_Patch), 1],
        [typeof(LaunchRocket_OnHandClick_Patch), -9],
        [typeof(LeakingRadiation_Update_Patch), 0],
        [typeof(MainGameController_StartGame_Patch), 1],
        [typeof(MeleeAttack_CanDealDamageTo_Patch), 4],
        [typeof(PDAScanner_Scan_Patch), 3],
        [typeof(PickPrefab_AddToContainerAsync_Patch), 4],
        [typeof(Player_OnKill_Patch), 0],
        [typeof(Respawn_Start_Patch), 3],
        [typeof(RocketConstructor_StartRocketConstruction_Patch), 3],
        [typeof(SpawnConsoleCommand_SpawnAsync_Patch), 2],
        [typeof(SpawnOnKill_OnKill_Patch), 3],
        [typeof(SubConsoleCommand_OnConsoleCommand_sub_Patch), 0],
        [typeof(SubRoot_OnPlayerEntered_Patch), 5],
        [typeof(Trashcan_Update_Patch), 4],
        [typeof(uGUI_OptionsPanel_AddAccessibilityTab_Patch), -10],
        [typeof(uGUI_PDA_Initialize_Patch), 2],
        [typeof(uGUI_PDA_SetTabs_Patch), 3],
        [typeof(uGUI_Pings_IsVisibleNow_Patch), 0],
        [typeof(uGUI_SceneIntro_HandleInput_Patch), -2],
        [typeof(uGUI_SceneIntro_IntroSequence_Patch), 8],
        [typeof(uSkyManager_SetVaryingMaterialProperties_Patch), 0],
        [typeof(Welder_Weld_Patch), 1],
        [typeof(Poop_Perform_Patch), 1],
        [typeof(SeaDragonMeleeAttack_OnTouchFront_Patch), 9],
        [typeof(SeaDragonMeleeAttack_SwatAttack_Patch), 4],
        [typeof(SeaTreaderSounds_SpawnChunks_Patch), 3],
        [typeof(Vehicle_TorpedoShot_Patch), 3],
        [typeof(SeamothTorpedo_Update_Patch), 0],
        [typeof(SeaTreader_UpdatePath_Patch), 0],
        [typeof(SeaTreader_UpdateTurning_Patch), 0],
        [typeof(SeaTreader_Update_Patch), 0],
        [typeof(StasisSphere_LateUpdate_Patch), 0],
        [typeof(WaterParkCreature_BornAsync_Patch), 6],
        [typeof(WaterParkCreature_ManagedUpdate_Patch), 2],
    ];

    [TestMethod]
    public void AllTranspilerPatchesHaveSanityTest()
    {
        Type[] allPatchesWithTranspiler = typeof(NitroxPatcher.Main).Assembly.GetTypes().Where(p => typeof(NitroxPatch).IsAssignableFrom(p) && p.IsClass).Where(x => x.GetMethod("Transpiler") != null).ToArray();

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
        FieldInfo targetMethodInfo = patchClassType.GetRuntimeFields().FirstOrDefault(x => string.Equals(x.Name.Replace("_", ""), "targetMethod", StringComparison.OrdinalIgnoreCase));
        if (targetMethodInfo == null)
        {
            Assert.Fail($"Could not find either \"TARGET_METHOD\" nor \"targetMethod\" inside {patchClassType.Name}");
        }

        MethodInfo targetMethod = targetMethodInfo.GetValue(null) as MethodInfo;
        List<CodeInstruction> originalIl = PatchTestHelper.GetInstructionsFromMethod(targetMethod).ToList();
        List<CodeInstruction> originalIlCopy = PatchTestHelper.GetInstructionsFromMethod(targetMethod).ToList(); // Our custom pattern matching replaces OpCode/Operand in place, therefor we need a copy to compare if changes are present

        MethodInfo transpilerMethod = patchClassType.GetMethod("Transpiler");
        if (transpilerMethod == null)
        {
            Assert.Fail($"Could not find \"Transpiler\" inside {patchClassType.Name}");
        }

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

        return myTypeBld.DefineMethod(method.Name, MethodAttributes.Public,  method.ReturnType, method.GetParameters().Types()).GetILGenerator();
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
