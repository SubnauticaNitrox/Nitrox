using System;

namespace NitroxClient.GameLogic.FMOD
{
    public class FMODSuppressor : IDisposable
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