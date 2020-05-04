using System.IO;

namespace NitroxServer.Serialization
{
    public interface IServerSerializer
    {
        void Serialize(Stream stream, object o);

        T Deserialize<T>(Stream stream);
    }
}
