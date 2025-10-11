using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;

namespace NitroxServer.ConsoleCommands.Abstract.Type;

public class TypeNitroxId(string name, bool isRequired, string description) : Parameter<NitroxId>(name, isRequired, description)
{
    public override bool IsValid(string arg)
    {
        return IsValid(arg, out _);
    }

    private static bool IsValid(string arg, out Guid result)
    {
        return Guid.TryParse(arg, out result);
    }

    public override NitroxId Read(string arg)
    {
        Validate.IsTrue(IsValid(arg, out Guid result), "Received an invalid NitroxId");
        return new NitroxId(result);
    }
}
