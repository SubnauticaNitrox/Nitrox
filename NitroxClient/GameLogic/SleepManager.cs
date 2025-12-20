using System.Diagnostics.CodeAnalysis;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic;

public class SleepManager
{
    [MemberNotNullWhen(true, nameof(currentBed))]
    public bool IsInBed => currentBed != null;

    public bool CanExitBed => IsInBed && !isSleepInProgress;

    private readonly IPacketSender packetSender;
    private Bed? currentBed;
    private bool isSleepInProgress;
    private float timeLastSleepBeforeEntering;

    public SleepManager(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public void LocalPlayerEnteredBed(Bed bed)
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

        currentBed.ExitInUseMode(Player.main, false);
        Player.main.timeLastSleep = timeLastSleepBeforeEntering;

        packetSender.Send(new BedExit());

        currentBed = null;
    }

    public void OnAllPlayersSleeping()
    {
        isSleepInProgress = true;
        uGUI_PlayerSleep.main.StartSleepScreen();
        DayNightCycle.main.SkipTime(396f, 5f);
    }

    public void OnSleepCancelled()
    {
        if (DayNightCycle.main.IsInSkipTimeMode())
        {
            DayNightCycle.main.StopSkipTimeMode();
        }

        if (IsInBed)
        {
            currentBed.ExitInUseMode(Player.main, false);
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
            currentBed.ExitInUseMode(Player.main, false);
        }

        currentBed = null;
        isSleepInProgress = false;
    }
}
