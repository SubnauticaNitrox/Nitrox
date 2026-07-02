using System;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets.Exceptions;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState;

public sealed class EstablishingSessionPolicy : ConnectionNegotiatingState
{
    public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY;

    public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        try
        {
            ValidateState(sessionConnectionContext);
            AwaitReservationCredentials(sessionConnectionContext);
        }
        catch (Exception)
        {
            Disconnect(sessionConnectionContext);
            throw;
        }
        return Task.CompletedTask;
    }

    private void ValidateState(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        SessionPolicyIsNotNull(sessionConnectionContext);
        SessionPolicyPacketCorrelation(sessionConnectionContext);
    }

    private static void SessionPolicyIsNotNull(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        try
        {
            Validate.NotNull(sessionConnectionContext.SessionPolicy);
        }
        catch (ArgumentNullException ex)
        {
            throw new InvalidOperationException("The context is missing a session policy.", ex);
        }
    }

    private void SessionPolicyPacketCorrelation(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        if (sessionConnectionContext.SessionPolicy.SessionId < 1)
        {
            throw new UncorrelatedPacketException(sessionConnectionContext.SessionPolicy, sessionConnectionContext.SessionPolicy.SessionId);
        }
    }

    private void AwaitReservationCredentials(IMultiplayerSessionConnectionContext sessionConnectionContext)
    {
        AwaitingReservationCredentials nextState = new();
        sessionConnectionContext.UpdateConnectionState(nextState);
    }
}
