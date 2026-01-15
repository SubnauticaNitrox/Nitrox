using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class StopCommand(IHostApplicationLifetime lifetimeService) : Command("stop", Perms.ADMIN, "Stops the server")
{
    private readonly IHostApplicationLifetime lifetimeService = lifetimeService;
    public override IEnumerable<string> Aliases { get; } = ["exit", "halt", "quit", "close"];

    protected override void Execute(CallArgs args)
    {
        lifetimeService.StopApplication();
    }
}
