using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.Surrogates;

#pragma warning disable SYSLIB0050 // Nitrox can depend on Binary serialization
public abstract class SerializationSurrogate<T> : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        GetObjectData((T)obj, info);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        return SetObjectData((T)obj, info);
    }

    protected abstract void GetObjectData(T obj, SerializationInfo info);

    protected abstract T SetObjectData(T obj, SerializationInfo info);
}
#pragma warning restore SYSLIB0050
