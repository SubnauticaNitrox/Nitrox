using System;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresPermissionAttribute(Perms minimumPermission) : Attribute
{
    /// <summary>
    ///     Gets the minimum permission needed to use this command.
    /// </summary>
    public Perms MinimumPermission { get; } = minimumPermission;
}
