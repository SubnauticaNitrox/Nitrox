using System;

namespace Nitrox.Model.Platforms.OS.Windows.Internal;

public enum PtraceRequest : int
{
    PTRACE_ATTACH = 16,
    PTRACE_DETACH = 17,
    PTRACE_POKEDATA = 5
}
