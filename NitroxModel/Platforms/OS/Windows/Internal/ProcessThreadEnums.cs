using System;

public enum PtraceRequest : int
{
    PTRACE_ATTACH = 16,
    PTRACE_DETACH = 17,
    PTRACE_POKEDATA = 5
}

[Flags]
public enum ThreadAccess : int
{
    SUSPEND_RESUME = 0x0002
}
