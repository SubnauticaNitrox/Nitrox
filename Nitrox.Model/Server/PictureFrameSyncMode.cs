namespace Nitrox.Model.Server;

public enum PictureFrameSyncMode
{
    // Don't do any sync between players
    OFF,
    
    // Sync between players for as long as the server stays up
    SESSION,
    
    // Persist it to disk as an aes gcm cipher so images survive restarts
    PERSISTED
}
