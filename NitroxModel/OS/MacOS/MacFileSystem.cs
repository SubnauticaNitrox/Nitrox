using System.Collections.Generic;

namespace NitroxModel.OS.MacOS
{
    public sealed class MacFileSystem : FileSystem
    {
        public override IEnumerable<string> GetDefaultPrograms(string file)
        {
            yield return "open";
        }
    }
}
