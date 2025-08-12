using System.Collections.Generic;
using NitroxServer.Helper;

namespace NitroxServer.Resources;

public class RandomSpawnSpoofer
{
    private readonly Dictionary<string, string[]> randomPossibilitiesByClassId;

    public RandomSpawnSpoofer(Dictionary<string, string[]> randomPossibilitiesByClassId)
    {
        this.randomPossibilitiesByClassId = randomPossibilitiesByClassId;
    }

    public void PickRandomClassIdIfRequired(ref string classId)
    {
        if (randomPossibilitiesByClassId.TryGetValue(classId, out string[] choices))
        {
            int randomIndex = XORRandom.NextIntRange(0, choices.Length);
            classId = choices[randomIndex];
        }
    }
}
