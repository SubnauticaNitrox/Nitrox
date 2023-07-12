using System;
using System.IO;

namespace NitroxServer.Serialization.World;

public class WorldManager
{
    public static readonly string SavesFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves");
    static WorldManager()
    {
        try
        {
            Directory.CreateDirectory(SavesFolderDir);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Couldn't create \"saves\" folder");
            throw new Exception(ex.ToString());
        }
    }
}
