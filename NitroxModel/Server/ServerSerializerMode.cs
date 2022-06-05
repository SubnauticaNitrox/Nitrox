using System;

namespace NitroxModel.Server
{
    public enum ServerSerializerMode
    {
        JSON
    }

    public static class ServerSerializerModeExtension
    {
        public static string GetFileEnding(this ServerSerializerMode mode)
        {
            return mode switch
            {
                ServerSerializerMode.JSON => ".json",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
