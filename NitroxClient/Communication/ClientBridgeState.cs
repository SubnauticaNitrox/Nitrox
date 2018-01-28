namespace NitroxClient.Communication
{
    public enum ClientBridgeState
    {
        Disconnected,
        WaitingForRerservation,
        Failed,
        Reserved,
        ReservationRejected,
        Connected
    }
}
