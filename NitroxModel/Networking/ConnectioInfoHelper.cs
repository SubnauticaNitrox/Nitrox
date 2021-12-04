using System;

namespace NitroxModel.Networking
{
    public static class ConnectionInfoHelper
    {

        /// <remarks>throws an ArgumentException if the connectionInfo is not the required type</remarks>
        /// <exception cref="ArgumentException"/>
        public static T RequireType<T>(IConnectionInfo connectionInfo) where T : class, IConnectionInfo
        {
            if (connectionInfo is not T typedConnection)
            {
                throw new ArgumentException("Tried parsing incorrect type of connectionInfo");
            }

            return typedConnection;
        }
    }
}

