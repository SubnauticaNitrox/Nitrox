namespace NitroxModel.Networking
{
    public class DirectConnection : IConnectionInfo
    {
        public string IPAddress { get; set; }
        public ushort Port { get; set; }

        public DirectConnection(string ip, ushort port)
        {
            IPAddress = ip;
            Port = port;
        }

        public override string ToString()
        {
            return $"{IPAddress}:{Port}";
        }

        public bool Equals(IConnectionInfo other)
        {
            DirectConnection otherConnection = other as DirectConnection;

            if (otherConnection == null)
            {
                return false;
            }

            return otherConnection.IPAddress == IPAddress && otherConnection.Port == Port;
        }

        public bool Equals(IConnectionInfo x, IConnectionInfo y)
        {
            return x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DirectConnection);
        }

        public override int GetHashCode()
        {
            int hashCode = 1666156946;
            hashCode = hashCode * -1521134295 + IPAddress.GetHashCode();
            hashCode = hashCode * -1521134295 + Port.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(DirectConnection left, DirectConnection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DirectConnection left, DirectConnection right)
        {
            return !(left == right);
        }
    }
}
