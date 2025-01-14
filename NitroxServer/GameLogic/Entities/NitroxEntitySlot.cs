using System;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Entities;

[Serializable]
public record struct NitroxEntitySlot(string BiomeType, List<string> AllowedTypes);
