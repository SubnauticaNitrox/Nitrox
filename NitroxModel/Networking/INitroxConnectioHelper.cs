using System;

namespace NitroxModel.Networking
{
    public static class IConnectionInfoHelper
    {

        /// <exception cref="NotSupportedException"/>
        public static T RequireType<T>(IConnectionInfo connectionInfo) where T : class, IConnectionInfo
        {

            T typedConnection = connectionInfo as T;
            if (typedConnection == null)
            {
                throw new NotSupportedException("Tried passing incorrect connectionInfo");
            }

            return typedConnection;
        }
    }
}

