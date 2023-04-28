using System;
using System.Collections.Generic;

namespace Nitrox.Test.Helper.Faker;

public class NitroxActionFaker : NitroxFaker, INitroxFaker
{
    private readonly Func<Bogus.Faker, object> generateAction;

    public NitroxActionFaker(Type type, Func<Bogus.Faker, object> action)
    {
        OutputType = type;
        generateAction = action;
    }

    public INitroxFaker[] GetSubFakers() => Array.Empty<INitroxFaker>();

    public object GenerateUnsafe(HashSet<Type> _) => generateAction.Invoke(Faker);
}
