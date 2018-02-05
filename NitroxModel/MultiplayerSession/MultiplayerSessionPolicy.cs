namespace NitroxModel.MultiplayerSession
{
    public class MultiplayerSessionPolicy
    {
        public bool RequiresServerPassword { get; }

        public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }

        public MultiplayerSessionPolicy()
        {
            //This is done intentionally. This is only a stub for future extension.
            RequiresServerPassword = false;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.Server;
        }
    }
}
