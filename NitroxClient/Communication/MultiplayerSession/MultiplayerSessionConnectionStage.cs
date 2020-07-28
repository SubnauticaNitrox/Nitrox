namespace NitroxClient.Communication.MultiplayerSession
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4016:Enumeration members should not be named \"Reserved\"", Justification = "<Pending>")]
    public enum MultiplayerSessionConnectionStage
    {
        DISCONNECTED,
        ESTABLISHING_SERVER_POLICY,
        AWAITING_RESERVATION_CREDENTIALS,
        AWAITING_SESSION_RESERVATION,
        SESSION_RESERVED,
        SESSION_RESERVATION_REJECTED,
        SESSION_JOINED
    }
}
