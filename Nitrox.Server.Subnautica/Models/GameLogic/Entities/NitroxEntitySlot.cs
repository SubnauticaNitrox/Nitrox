using System;
using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

[Serializable]
public record struct NitroxEntitySlot(string BiomeType, List<string> AllowedTypes);
