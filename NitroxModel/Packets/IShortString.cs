namespace NitroxModel.Packets
{
    public interface IShortString
    {
        /// <summary>
        ///     Gets a short summary of the packet to quickly see what's in it.
        /// </summary>
        /// <returns>A short summary of the packet data as string.</returns>
        string ToShortString();
    }
}
