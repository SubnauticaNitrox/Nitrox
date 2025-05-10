using ProtoBufNet;

namespace NitroxServer.UnityStubs;

[ProtoContract]
public class GameObjectData
{
    [ProtoMember(1)]
    public bool CreateEmptyObject
    {
        get;
        set;
    }

    [ProtoMember(2)]
    public bool IsActive
    {
        get;
        set;
    }

    [ProtoMember(3)]
    public int Layer
    {
        get;
        set;
    }

    [ProtoMember(4)]
    public string Tag
    {
        get;
        set;
    }

    [ProtoMember(6)]
    public string Id
    {
        get;
        set;
    }

    [ProtoMember(7)]
    public string ClassId
    {
        get;
        set;
    }

    [ProtoMember(8)]
    public string Parent
    {
        get;
        set;
    }

    [ProtoMember(9)]
    public bool OverridePrefab
    {
        get;
        set;
    }

    [ProtoMember(10)]
    public bool MergeObject
    {
        get;
        set;
    }

    public void Reset()
    {
        CreateEmptyObject = false;
        IsActive = false;
        Layer = 0;
        Tag = null;
        Id = null;
        ClassId = null;
        Parent = null;
        OverridePrefab = false;
        MergeObject = false;
    }

    public override string ToString()
    {
        return string.Format("(CreateEmptyObject={0}, IsActive={1}, Layer={2}, Tag={3}, Id={4}, ClassId={5}, Parent={6}, OverridePrefab={7}, MergeObject={8})", new object[]
        {
            CreateEmptyObject,
            IsActive,
            Layer,
            Tag,
            Id,
            ClassId,
            Parent,
            OverridePrefab,
            MergeObject
        });
    }
}
