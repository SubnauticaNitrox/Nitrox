using System;

namespace Nitrox.Launcher.Models.Design;

[Serializable]
public record NitroxChangelog(string Version, DateTime Released, string PatchNotes);
