using System.IO;

namespace NitroxServer.Serialization
{
    public interface IServerSerializer
    {
        void Serialize(Stream stream, object o);
        void Serialize(string filePath, object o);

        T Deserialize<T>(Stream stream);
        T Deserialize<T>(string filePath);
    }
}
