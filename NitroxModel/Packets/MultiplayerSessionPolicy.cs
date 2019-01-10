﻿using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MultiplayerSessionPolicy : CorrelatedPacket
    {
        public bool RequiresServerPassword { get; }

        public MultiplayerSessionAuthenticationAuthority AuthenticationAuthority { get; }

        public string NitroxVersionAllowed { get; }

        public MultiplayerSessionPolicy(string correlationId)
            : base(correlationId)
        {
            // This is done intentionally. It is only a stub for future extension.
            RequiresServerPassword = false;
            AuthenticationAuthority = MultiplayerSessionAuthenticationAuthority.Server;
            // get the full version name
            NitroxVersionAllowed = typeof(MultiplayerSessionPolicy).Assembly.FullName;
            // restrict to only the major and minor version number is required
            NitroxVersionAllowed = System.Text.RegularExpressions.Regex.Match(NitroxVersionAllowed, @"(\d+\.\d+)").Value;
        }
    }
}
