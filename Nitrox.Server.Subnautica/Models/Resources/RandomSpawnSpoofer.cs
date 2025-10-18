using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.Resources;

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
            int randomIndex = XorRandom.NextIntRange(0, choices.Length);
            classId = choices[randomIndex];
        }
    }
}
