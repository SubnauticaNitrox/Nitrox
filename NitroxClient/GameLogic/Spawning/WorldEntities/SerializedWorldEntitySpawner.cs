using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class SerializedWorldEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    /// <summary>
    /// Contains the only types we allow the server to instantiate on clients (for security concerns)
    /// </summary>
    private readonly HashSet<Type> typesWhitelist = new()
    {
        typeof(Light), typeof(DisableBeforeExplosion), typeof(BoxCollider), typeof(SphereCollider)
    };

    public SerializedWorldEntitySpawner()
    {
        // Preloading a useful asset
        if (!NitroxEnvironment.IsTesting && !ProtobufSerializer.emptyGameObjectPrefab)
        {
            ProtobufSerializer.emptyGameObjectPrefab = Resources.Load<GameObject>("SerializerEmptyGameObject");
        }
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, parent, cellRoot, result);
        yield break;
    }

    public bool SpawnsOwnChildren() => false;

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not SerializedWorldEntity serializedWorldEntity)
        {
            return true;
        }

        using PooledObject<ProtobufSerializer> proxy = ProtobufSerializerPool.GetProxy();
        ProtobufSerializer serializer = proxy.Value;

        UniqueIdentifier uniqueIdentifier = serializer.CreateEmptyGameObject("SerializerEmptyGameObject");
        GameObject gameObject = uniqueIdentifier.gameObject;
        gameObject.SetActive(false);
        gameObject.layer = serializedWorldEntity.Layer;
        gameObject.tag = "Untagged"; // Same tag for all empty game objects

        LargeWorldEntity largeWorldEntity = gameObject.AddComponent<LargeWorldEntity>();
        largeWorldEntity.cellLevel = (LargeWorldEntity.CellLevel)serializedWorldEntity.Level;

        Transform transform = gameObject.transform;
        transform.SetParent(cellRoot.liveRoot.transform);
        NitroxVector3 localPosition = serializedWorldEntity.Transform.LocalPosition - serializedWorldEntity.AbsoluteEntityCell.Position;
        transform.localPosition = localPosition.ToUnity();
        transform.localRotation = serializedWorldEntity.Transform.LocalRotation.ToUnity();
        transform.localScale = serializedWorldEntity.Transform.LocalScale.ToUnity();

        // Code inspired from ProtobufSerializer.DeserializeIntoGameObject
        Dictionary<Type, int> dictionary = ProtobufSerializer.componentCountersPool.Get();
        dictionary.Clear();
        foreach (SerializedComponent serializedComponent in serializedWorldEntity.Components)
        {
            string typeName = serializedComponent.TypeName;
            Type cachedType = ProtobufSerializer.GetCachedType(typeName);
            if (!typesWhitelist.Contains(cachedType))
            {
                Log.ErrorOnce($"Server asked to instantiate a non-whitelisted type {typeName}.");
                return true;
            }

            using MemoryStream stream = new(serializedComponent.Data);
            int id = ProtobufSerializer.IncrementComponentCounter(dictionary, cachedType);
            Component orAddComponent = ProtobufSerializer.GetOrAddComponent(gameObject, cachedType, typeName, id, true);
            if (orAddComponent)
            {
                serializer.Deserialize(stream, orAddComponent, cachedType, false);
            }
            else
            {
                Log.ErrorOnce($"Deserializing component {typeName} into {gameObject} failed");
            }
            ProtobufSerializer.SetIsEnabled(orAddComponent, serializedComponent.IsEnabled);
        }
        foreach (IProtoEventListener listener in gameObject.GetComponents<IProtoEventListener>())
        {
            listener.OnProtoDeserialize(serializer);
        }
        dictionary.Clear();
        ProtobufSerializer.componentCountersPool.Return(dictionary);

        gameObject.SetActive(true);
        
        result.Set(gameObject);
        return true;
    }
}
