using System;
using System.Runtime.CompilerServices;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.Helpers;

public static class NitroxEntityExtensions
{
    public static bool TryGetNitroxEntity(this Component component, out NitroxEntity nitroxEntity)
    {
        nitroxEntity = null;
        return component && component.TryGetComponent(out nitroxEntity);
    }

    public static bool TryGetNitroxEntity(this GameObject gameObject, out NitroxEntity nitroxEntity)
    {
        nitroxEntity = null;
        return gameObject && gameObject.TryGetComponent(out nitroxEntity);
    }

    public static bool TryGetNitroxId(this GameObject gameObject, out NitroxId nitroxId)
    {
        if (!gameObject || !gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            nitroxId = null;
            return false;
        }

        nitroxId = nitroxEntity.Id;
        return true;
    }

    public static bool TryGetNitroxId(this Component component, out NitroxId nitroxId)
    {
        if (!component || !component.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            nitroxId = null;
            return false;
        }

        nitroxId = nitroxEntity.Id;
        return true;
    }

    public static bool TryGetIdOrWarn(
        this GameObject gameObject,
        out NitroxId nitroxId,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        // Since a destroyed but non-null object is normal behavior for Unity, we don't want to warn about it.
        if (!gameObject)
        {
            nitroxId = null;
            return false;
        }
        if (!gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            Log.Warn($"[{filePath[(filePath.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^2] + methodName}():L{lineNumber}] Couldn't find an id on {gameObject.GetFullHierarchyPath()}");
            nitroxId = null;
            return false;
        }

        nitroxId = nitroxEntity.Id;
        return true;
    }

    public static bool TryGetIdOrWarn(
        this Component component,
        out NitroxId nitroxId,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        // Since a destroyed but non-null object is normal behavior for Unity, we don't want to warn about it.
        if (!component)
        {
            nitroxId = null;
            return false;
        }
        if (!component.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            Log.Warn($"[{filePath[(filePath.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^2] + methodName}():L{lineNumber}] Couldn't find an id on {component.GetFullHierarchyPath()}");
            nitroxId = null;
            return false;
        }

        nitroxId = nitroxEntity.Id;
        return true;
    }

    public static Optional<NitroxId> GetId(this GameObject gameObject)
    {
        if (gameObject && gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            return Optional.Of(nitroxEntity.Id);
        }

        return Optional.Empty;
    }

    public static Optional<NitroxId> GetId(this Component component)
    {
        if (component && component.TryGetComponent(out NitroxEntity nitroxEntity))
        {
            return Optional.Of(nitroxEntity.Id);
        }

        return Optional.Empty;
    }
}
