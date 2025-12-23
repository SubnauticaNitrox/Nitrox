using System.Diagnostics.CodeAnalysis;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.GameLogic;

public class SleepManager(IPacketSender packetSender)
{
    private readonly IPacketSender packetSender = packetSender;
    private Bed? currentBed;
    private bool isSleepInProgress;
    private float timeLastSleepBeforeEntering;

    [MemberNotNullWhen(true, nameof(currentBed))]
    private bool IsInBed => currentBed != null;

    public bool CanExitBed => IsInBed && !isSleepInProgress;

    public void EnterBed(Bed bed)
    {
        currentBed = bed;
        isSleepInProgress = false;
        timeLastSleepBeforeEntering = Player.main.timeLastSleep;
    }

    public void ExitBed()
    {
        if (!IsInBed || isSleepInProgress)
        {
            return;
        }

        currentBed.ExitInUseMode(Player.main);
        Player.main.timeLastSleep = timeLastSleepBeforeEntering;
        packetSender.Send(new BedExit());
        currentBed = null;
    }

    public void OnAllPlayersSleeping()
    {
        isSleepInProgress = true;
        uGUI_PlayerSleep.main.StartSleepScreen();
    }

    public void OnSleepCancelled()
    {
        if (IsInBed)
        {
            currentBed.ExitInUseMode(Player.main);
            Player.main.timeLastSleep = timeLastSleepBeforeEntering;
        }

        uGUI_PlayerSleep.main.StopSleepScreen();
        currentBed = null;
        isSleepInProgress = false;
    }

    public void OnSleepComplete()
    {
        if (IsInBed)
        {
            currentBed.ExitInUseMode(Player.main);
        }

        currentBed = null;
        isSleepInProgress = false;
    }
}
