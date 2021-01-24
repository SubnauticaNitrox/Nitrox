using System;
using System.Collections.Generic;

namespace NitroxClient.Persistence.Model
{
    [Serializable]
    class PersistedClientDataModel
    {
        public List<SavedServer> SavedServers { get; set; } = new List<SavedServer>();
    }
}
