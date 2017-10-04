using NitroxModel.DataStructures.Surrogates;
using NitroxModel.Logger;
using NitroxModel.Tcp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static readonly SurrogateSelector surrogateSelector;
        private static readonly StreamingContext streamingContext;

        static Packet()
        {
            surrogateSelector = new SurrogateSelector();
            streamingContext = new StreamingContext(StreamingContextStates.All); // Our surrogates can be safely used in every context.

            var types = Assembly.GetExecutingAssembly()
                .GetTypes();

            types.Where(t =>
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(SerializationSurrogate<>) &&
                    t.IsClass &&
                    !t.IsAbstract)
                .ForEach(t =>
                {
                    var surrogate = (ISerializationSurrogate)Activator.CreateInstance(t);
                    var surrogatedType = t.BaseType.GetGenericArguments()[0];
                    surrogateSelector.AddSurrogate(surrogatedType, streamingContext, surrogate);

                    Log.Info("Added surrogate " + surrogate + " for type " + surrogatedType);
                });

            // For completeness, we could pass a StreamingContextStates.CrossComputer.
            Serializer = new BinaryFormatter(surrogateSelector, streamingContext);
        }

        public static readonly BinaryFormatter Serializer = null;

        public byte[] SerializeWithHeaderData()
        {
            byte[] packetData;

            using (MemoryStream ms = new MemoryStream())
            {
                //place holder for size, will be filled in later... allows us
                //to avoid doing a byte array merge... zomg premature optimization
                ms.Write(new Byte[MessageBuffer.HEADER_BYTE_SIZE], 0, MessageBuffer.HEADER_BYTE_SIZE);
                Serializer.Serialize(ms, this);
                packetData = ms.ToArray();
            }

            int packetSize = packetData.Length - MessageBuffer.HEADER_BYTE_SIZE; // subtract HEADER_BYTE_SIZE because we dont want to take into account the added bytes
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

            //premature optimization continued :)
            for (int i = 0; i < MessageBuffer.HEADER_BYTE_SIZE; i++)
            {
                packetData[i] = packetSizeBytes[i];
            }

            return packetData;
        }
    }
}
