namespace NitroxModel.Constants
{
    /// <summary>
    ///     Shared values across LANDiscoveryClient, in NitroxClient project and LANDiscoveryServer in NitroxServer project.
    /// </summary>
    public static class LANDiscoveryConstants
    {
        public static readonly int[] BROADCAST_PORTS = { 1467, 8710, 16723, 3813, 9704 }; // Randomly generated
        public const string BROADCAST_REQUEST_STRING = "NitroxLANRequest";
        public const string BROADCAST_RESPONSE_STRING = "NitroxLANResponse";
    }
}
