using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.New;

public class BuildingTester : MonoBehaviour
{
    public string TargetBaseName;
    public Base TargetBase;
    public int Cycle = 0;
    public int Slot = 0;

    private JsonSerializer serializer;
    public string SavePath = Path.Combine(NitroxUser.LauncherPath, "SavedBases");

    public void OnEnable()
    {
        serializer = new();
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.TypeNameHandling = TypeNameHandling.Auto;
        CycleTargetBase();
    }

    public void CycleTargetBase()
    {
        Base[] bases = LargeWorldStreamer.main.globalRoot.GetComponentsInChildren<Base>(true);
        if (Cycle >= bases.Length - 1)
        {
            Cycle = 0;
        }
        else
        {
            Cycle++;
        }
        if (Cycle < bases.Length)
        {
            TargetBase = bases[Cycle];
            TargetBaseName = TargetBase.name;
        }
        else
        {
            TargetBase = null;
            TargetBaseName = string.Empty;
        }
    }

    public void SaveGlobalRoot()
    {
        SavedGlobalRoot savedGlobalRoot = NitroxGlobalRoot.From(LargeWorldStreamer.main.globalRoot.transform);
        using StreamWriter writer = File.CreateText($"{SavePath}\\big-save{Slot}.json");
        serializer.Serialize(writer, savedGlobalRoot);
    }

    public void LoadGlobalRoot()
    {
        DateTimeOffset beginTime = DateTimeOffset.Now;
        using StreamReader reader = File.OpenText($"{SavePath}\\big-save{Slot}.json");
        SavedGlobalRoot savedGlobalRoot = (SavedGlobalRoot)serializer.Deserialize(reader, typeof(SavedGlobalRoot));
        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug($"Took {(endTime - beginTime).TotalMilliseconds}ms to deserialize the SavedGlobalRoot\n{savedGlobalRoot}");
        StartCoroutine(LoadGlobalRootAsync(savedGlobalRoot));
    }

    public void ResetGlobalRoot()
    {
        Transform globalRoot = LargeWorldStreamer.main.globalRoot.transform;
        for (int i = globalRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(globalRoot.GetChild(i).gameObject);
        }
    }

    private IEnumerator LoadGlobalRootAsync(SavedGlobalRoot savedGlobalRoot)
    {
        DateTimeOffset beginTime = DateTimeOffset.Now;
        foreach (SavedBuild build in savedGlobalRoot.Builds)
        {
            yield return LoadBaseAsync(build);
        }
        yield return NitroxBuild.RestoreModules(LargeWorldStreamer.main.globalRoot.transform, savedGlobalRoot.Modules);
        yield return NitroxBuild.RestoreGhosts(LargeWorldStreamer.main.globalRoot.transform, savedGlobalRoot.Ghosts);
        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug($"Took {(endTime - beginTime).TotalMilliseconds}ms to fully load the SavedGlobalRoot");
    }

    public void LogCurrentBase()
    {
        Log.Debug($"{NitroxBase.From(TargetBase)}");
        if (TargetBase.TryGetComponentInParent(out ConstructableBase constructableBase))
        {
            foreach (Renderer renderer in constructableBase.ghostRenderers)
            {
                Log.Debug($"{renderer.name} (under {renderer.transform.parent.name}) bounds: {renderer.bounds}");
            }
        }
    }

    public void LogAndSaveCurrentBase()
    {
        if (!TargetBase.AliveOrNull())
        {
            Log.Debug("Base is null or not alive");
            return;
        }

        SavedBuild saved = NitroxBuild.From(TargetBase);
        Log.Debug($"Saved base: {TargetBase.name}\n{saved}");
        SaveBase(saved);
    }

    private void SaveBase(SavedBuild savedBuild)
    {
        using StreamWriter writer = File.CreateText($"{SavePath}\\save{Slot}.json");
        serializer.Serialize(writer, savedBuild);
    }

    public void LoadBase()
    {
        DateTimeOffset beginTime = DateTimeOffset.Now;
        using StreamReader reader = File.OpenText($"{SavePath}\\save{Slot}.json");
        SavedBuild savedBuild = (SavedBuild)serializer.Deserialize(reader, typeof(SavedBuild));
        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug($"Took {(endTime - beginTime).TotalMilliseconds}ms to deserialize the SavedBuild\n:{savedBuild}");
        StartCoroutine(LoadBaseAsync(savedBuild));
    }

    private IEnumerator LoadBaseAsync(SavedBuild savedBuild)
    {
        DateTimeOffset beginTime = DateTimeOffset.Now;
        yield return CreateBase(savedBuild);
        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug($"Took {(endTime - beginTime).TotalMilliseconds}ms to create the Base");
    }

    private IEnumerator CreateBase(SavedBuild savedBuild)
    {
        // Like in BaseGhost.Finish()
        GameObject newBase = Instantiate(BaseGhost._basePrefab, LargeWorldStreamer.main.globalRoot.transform, savedBuild.Position.ToUnity(), savedBuild.Rotation.ToUnity(), savedBuild.LocalScale.ToUnity(), false);
        if (LargeWorld.main)
        {
            LargeWorld.main.streamer.cellManager.RegisterEntity(newBase);
        }
        Base @base = newBase.GetComponent<Base>();
        if (!@base)
        {
            Log.Debug("No Base component found");
            yield break;
        }

        yield return savedBuild.ApplyToAsync(@base);
        newBase.SetActive(true);
        yield return savedBuild.RestoreGhosts(@base);
        @base.OnProtoDeserialize(null);
        @base.FinishDeserialization();
    }

    public void DeleteTargetBase()
    {
        if (!TargetBase)
        {
            return;
        }
        Destroy(TargetBase.gameObject);
    }
}
