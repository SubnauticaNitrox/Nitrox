using System;

namespace NitroxClient.GameLogic.FMOD
{
    public readonly struct FMODSuppressor : IDisposable
    {
        public static bool SuppressFMODEvents;

        public FMODSuppressor()
        {
            SuppressFMODEvents = true;
        }

        public void Dispose()
        {
            SuppressFMODEvents = false;
        }
    }
}
