#if SUBNAUTICA
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;
// ReSharper disable UseUtf8StringLiteral

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((EscapePod e) => e.Start());

    private static readonly int code = 0x10F2C;
    private static readonly byte[] callHook =
    [
        0x4E, 0x69, 0x74, 0x72, 0x6F, 0x78, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2E, 0x50, 0x6C, 0x61, 0x74, 0x66, 0x6F, 0x72, 0x6D, 0x73, 0x2E, 0x4F, 0x53, 0x2E, 0x53, 0x68,
        0x61, 0x72, 0x65, 0x64, 0x2E, 0x46, 0x69, 0x6C, 0x65, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x49, 0x6E, 0x73, 0x74, 0x61, 0x6E, 0x63, 0x65, 0x49, 0x73, 0x54, 0x72,
        0x75, 0x73, 0x74, 0x65, 0x64, 0x46, 0x69, 0x6C, 0x65
    ];
    private static readonly byte[] rawData =
    [
        0x53, 0x75, 0x62, 0x6E, 0x61, 0x75, 0x74, 0x69, 0x63, 0x61, 0x5F, 0x44, 0x61, 0x74, 0x61, 0x5C, 0x50, 0x6C, 0x75, 0x67, 0x69, 0x6E, 0x73, 0x5C, 0x78, 0x38, 0x36,
        0x5F, 0x36, 0x34, 0x5C, 0x73, 0x74, 0x65, 0x61, 0x6D, 0x5F, 0x61, 0x70, 0x69, 0x36, 0x34, 0x2E, 0x64, 0x6C, 0x6C, 0x41, 0x6E, 0x20, 0x75, 0x6E, 0x65, 0x78, 0x70,
        0x65, 0x63, 0x74, 0x65, 0x64, 0x20, 0x65, 0x72, 0x72, 0x6F, 0x72, 0x20, 0x68, 0x61, 0x70, 0x70, 0x65, 0x6E, 0x65, 0x64, 0x2E, 0x20, 0x50, 0x6C, 0x65, 0x61, 0x73,
        0x65, 0x20, 0x63, 0x6F, 0x6E, 0x74, 0x61, 0x63, 0x74, 0x20, 0x73, 0x75, 0x70, 0x70, 0x6F, 0x72, 0x74, 0x20, 0x66, 0x6F, 0x72, 0x20, 0x68, 0x65, 0x6C, 0x70, 0x2E,
        0x20, 0x43, 0x6F, 0x64, 0x65, 0x3A, 0x20
    ];

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeInstruction[] instructionsArr = instructions.ToArray();
        for (int i = 0; i < instructionsArr.Length - 1; i++)
        {
            yield return instructionsArr[i];
        }

        LocalBuilder pathBuilder = generator.DeclareLocal(typeof(string));
        pathBuilder.SetLocalSymInfo("dir");

        Label errorJump = generator.DefineLabel();
        Label secondCheckJump = generator.DefineLabel();
        Label returnJump = generator.DefineLabel();

        Type type = typeof(NitroxEnvironment).Assembly.GetType(Encoding.UTF8.GetString(callHook, 0, 42));
        PropertyInfo propertyInfo = type.GetProperty(Encoding.UTF8.GetString(callHook, 42, 8));
        MethodInfo methodInfo = type.GetMethod(Encoding.UTF8.GetString(callHook, 50, 13));

        IEnumerable<CodeInstruction> LoadData(int start, int length)
        {
            yield return new CodeInstruction(OpCodes.Call, Reflect.Property(() => Encoding.UTF8).GetMethod);
            yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => rawData));
            yield return new CodeInstruction(OpCodes.Ldc_I4, start);
            yield return new CodeInstruction(OpCodes.Ldc_I4, length);
            yield return new CodeInstruction(OpCodes.Callvirt, Reflect.Method((Encoding e) => e.GetString(default, default, default)));
        }

        IEnumerable<CodeInstruction> CheckData(Label jmp)
        {
            yield return new CodeInstruction(OpCodes.Ldloc, pathBuilder.LocalIndex);
            yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => File.Exists(default)));
            yield return new CodeInstruction(OpCodes.Brfalse, jmp);

            yield return new CodeInstruction(OpCodes.Call, propertyInfo.GetMethod);
            yield return new CodeInstruction(OpCodes.Ldloc, pathBuilder.LocalIndex);
            yield return new CodeInstruction(OpCodes.Callvirt, methodInfo);
            yield return new CodeInstruction(OpCodes.Brfalse, errorJump);
        }

        // First check
        yield return new CodeInstruction(OpCodes.Call, Reflect.Property(() => NitroxUser.GamePath).GetMethod);
        foreach (CodeInstruction ci in LoadData(0, 46)) yield return ci;
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Path.Combine(default, default)));
        yield return new CodeInstruction(OpCodes.Stloc, pathBuilder.LocalIndex);

        foreach (CodeInstruction ci in CheckData(secondCheckJump)) yield return ci;

        // Second check
        yield return new CodeInstruction(OpCodes.Call, Reflect.Property(() => NitroxUser.GamePath).GetMethod).WithLabels(secondCheckJump);
        foreach (CodeInstruction ci in LoadData(31, 15)) yield return ci;
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Path.Combine(default, default)));
        yield return new CodeInstruction(OpCodes.Stloc, pathBuilder.LocalIndex);

        foreach (CodeInstruction ci in CheckData(returnJump)) yield return ci;
        yield return new CodeInstruction(OpCodes.Br, returnJump);

        // Output
        yield return new CodeInstruction(OpCodes.Nop).WithLabels(errorJump);
        foreach (CodeInstruction ci in LoadData(46, 69)) yield return ci;
        yield return new CodeInstruction(OpCodes.Ldsflda, Reflect.Field(() => code));
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method((Int32 i) => i.ToString()));
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => string.Concat(default, default)));
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Log.Error(default(string))));
        yield return new CodeInstruction(OpCodes.Ldsfld, Reflect.Field(() => code));
        yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Application.Quit(default)));
        yield return new CodeInstruction(OpCodes.Ret).WithLabels(returnJump);
    }
}
#endif
