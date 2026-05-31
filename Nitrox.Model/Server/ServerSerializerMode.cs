namespace Nitrox.Model.Server;

// TODO: Delete this. Saving should be done via SQL or JSON without letting user decide save format.
public enum ServerSerializerMode
{
    PROTOBUF,
    JSON
}
