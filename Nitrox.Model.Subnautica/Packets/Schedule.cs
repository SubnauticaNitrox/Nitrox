using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class Schedule : Packet
{
    public float TimeExecute { get; }
    public string Key { get; }
    public int Type { get; }
    
    public Schedule(float timeExecute, string key, int type)
    {
        TimeExecute = timeExecute;
        Key = key;
        Type = type;
    }
}
