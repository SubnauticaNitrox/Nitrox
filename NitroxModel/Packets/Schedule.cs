using System;

namespace NitroxModel.Packets;

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
