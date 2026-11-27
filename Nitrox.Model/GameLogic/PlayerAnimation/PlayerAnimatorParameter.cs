using System;
using System.Collections.Generic;

namespace Nitrox.Model.GameLogic.PlayerAnimation;

public static class PlayerAnimatorParameter
{
    public static readonly IReadOnlyList<string> SynchronizedParameters = Array.AsReadOnly(new[]
    {
        "using_pda",
        "using_tool",
        "using_tool_alt",
        "on_surface",
        "diving",
        "jump",
        "grab",
        "bash",
        "reloading_tool",
        "charged_stasisrifle",
        "bleeder"
    });

    private static readonly HashSet<string> synchronizedParameters = new(SynchronizedParameters, StringComparer.Ordinal);

    public static bool IsSynchronized(string parameterName)
    {
        return synchronizedParameters.Contains(parameterName);
    }
}
