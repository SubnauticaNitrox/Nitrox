using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public sealed class NitroxObject : ISerializable, IEquatable<NitroxObject>, IEqualityComparer<NitroxObject>, IDisposable
    {
        private static readonly Dictionary<NitroxId, NitroxObject> nitroxObjectsById = new Dictionary<NitroxId, NitroxObject>();

        [ProtoMember(1)]
        public NitroxTransform Transform => (NitroxTransform)behaviors[typeof(NitroxTransform)].First();

        private readonly Dictionary<Type, HashSet<NitroxBehavior>> behaviors = new Dictionary<Type, HashSet<NitroxBehavior>>();

        [ProtoMember(2)]
        private List<NitroxBehavior> serializableBehaviors;

        [ProtoMember(3)]
        private bool exists;

        [ProtoMember(4)]
        private NitroxId id;

        public NitroxId Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id != null)
                {
                    nitroxObjectsById.Remove(value);
                    nitroxObjectsById.Add(value, this);
                }

                id = value;
            }
        }

        internal HashSet<NitroxObject> Children = new HashSet<NitroxObject>();


        public NitroxObject()
        {
            NitroxTransform transform = new NitroxTransform(NitroxVector3.Zero, new NitroxQuaternion(0, 0, 0, 1), NitroxVector3.One);

            AddBehavior(transform);

            Id = new NitroxId();
            exists = true;
        }

        public NitroxObject(NitroxId id)
        {
            NitroxTransform transform = new NitroxTransform(NitroxVector3.Zero, new NitroxQuaternion(0, 0, 0, 1), NitroxVector3.One);

            AddBehavior(transform);

            Id = id;
            exists = true;
        }

        private NitroxObject(SerializationInfo info, StreamingContext context)
        {
            Id = (NitroxId)info.GetValue("id", typeof(NitroxId));
            exists = info.GetBoolean("exists"); // honestly should always exist if its getting serialized but just in case
            
            int count = info.GetInt32("behaviorsCount");
            for (int i = 0; i < count; i++)
            {
                AddBehavior((NitroxBehavior)info.GetValue($"behavior {i}", typeof(NitroxBehavior)));
            }

            count = info.GetInt32("childrenCount");
            for (int i = 0; i < count; i++)
            {
                NitroxObject child = (NitroxObject)info.GetValue($"child {i}", typeof(NitroxObject));
                child.Transform.SetParent(Transform);
                Children.Add(child);
            }
        }

        public List<NitroxObject> GetChildren()
        {
            return Children.ToList();
        }

        /// <summary>
        /// Gets behavior off of Object if it exists
        /// </summary>
        /// <returns>NitroxBehavior or null</returns>
        public T GetBehavior<T>() where T : NitroxBehavior
        {
            if (behaviors.TryGetValue(typeof(T), out HashSet<NitroxBehavior> val))
            {
                return (T)val.First();
            }
            return null;
        }

        public List<NitroxBehavior> GetBehaviors<T>() where T : NitroxBehavior
        {
            if (behaviors.TryGetValue(typeof(T), out HashSet<NitroxBehavior> val))
            {
                return val.ToList();
            }
            return new List<NitroxBehavior>();
        }

        public T AddBehavior<T>() where T : NitroxBehavior
        {
            T createdType = Activator.CreateInstance<T>();
            createdType.NitroxObject = this;
            if (behaviors.TryGetValue(typeof(T), out HashSet<NitroxBehavior> val))
            {
                val.Add(createdType);
            }
            else
            {
                val = new HashSet<NitroxBehavior>
                {
                    createdType
                };
                behaviors.Add(typeof(T), val);
            }

            return createdType;
        }

        public void AddBehavior(NitroxBehavior behavior)
        {
            behavior.NitroxObject = this;
            if (behaviors.TryGetValue(behavior.GetType(), out HashSet<NitroxBehavior> val))
            {
                val.Add(behavior);
            }
            else
            {
                val = new HashSet<NitroxBehavior>
                {
                    behavior
                };
                behaviors.Add(behavior.GetType(), val);
            }
        }

        [ProtoAfterDeserialization]
        private void AfterProtoDeserialize()
        {
            behaviors.Remove(typeof(NitroxTransform));
            foreach (NitroxBehavior behavior in serializableBehaviors)
            {
                AddBehavior(behavior);
            }
        }

        [ProtoBeforeSerialization]
        private void BeforeProtoSerializaition()
        {
            serializableBehaviors = behaviors.Values.SelectMany(s => s).ToList();
        }

        /// <summary>
        /// Mark object as deleted
        /// </summary>
        public void Delete()
        {
            exists = false;
            nitroxObjectsById.Remove(id);
        }

        public static bool operator ==(NitroxObject lh, object rh)
        {
            if (lh is null && rh is null)
            {
                return true;
            }
            else if (!(lh is null) && lh.exists && rh is object && rh is NitroxObject)
            {
                NitroxObject rhObject = rh as NitroxObject;

                return rhObject.exists && lh.Equals(rhObject);
            }

            return !(lh is null) && !lh.exists && rh is null;
        }

        public static bool operator !=(NitroxObject lh, object rh)
        {
            return !(lh == rh);
        }

        /// <summary>
        /// Gets a NitroxObject by its Id
        /// </summary>
        /// <param name="id">Id of object to get</param>
        /// <returns>NitroxObject or null</returns>
        public static NitroxObject GetObjectById(NitroxId id)
        {
            nitroxObjectsById.TryGetValue(id, out NitroxObject nitroxObject);
            return nitroxObject;
        }
        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public bool Equals(NitroxObject other)
        {
            return (!exists && !other.exists) || EqualityComparer<NitroxId>.Default.Equals(id, other.id);
        }

        public bool Equals(NitroxObject x, NitroxObject y)
        {
            return (x == null && y == null) || x != null && x.Equals(y);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public int GetHashCode(NitroxObject obj)
        {
            return 1877310944 + EqualityComparer<NitroxId>.Default.GetHashCode(obj.id);
        }

        public override string ToString()
        {
            return $"[ NitroxObject: Id: {Id}, Transform: {Transform}, Children: {string.Join(", ", Children)}";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id, typeof(NitroxId));
            info.AddValue("exists", exists); // honestly should always exist if its getting serialized but just in case

            NitroxBehavior[] behaviorsSerializing = behaviors.Values.SelectMany(x => x).ToArray();
            int count = behaviorsSerializing.Length;
            info.AddValue("behaviorsCount", count);
            for (int i = 0; i < count; i++)
            {
                info.AddValue($"behavior {i}", behaviorsSerializing[i]);
            }

            count = Children.Count;
            info.AddValue("childrenCount", count);
            List<NitroxObject> childrenSerializing = Children.ToList();
            for (int i = 0; i < count; i++)
            {
                info.AddValue($"child {i}", childrenSerializing[i]);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NitroxObject()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
