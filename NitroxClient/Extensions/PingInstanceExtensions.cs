namespace NitroxClient.Extensions;

internal static class PingInstanceExtensions
{
    extension(PingInstance self)
    {
        /// <summary>
        ///     If true, ping instance should not be synchronized to remote players.
        /// </summary>
        public bool IsLocalOnly
        {
            set => self._id = "local";
            get => self._id == "local";
        }
    }
}
