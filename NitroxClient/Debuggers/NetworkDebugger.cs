using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.Debuggers
{
    public class NetworkDebugger : BaseDebugger
    {
        public NetworkDebugger() : base(400, null, KeyCode.N, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            Tabs.Add("Packets");
        }

        private enum TabTypes
        {
            PACKETS
        }

        protected override void Render(int windowId)
        {
            switch ((TabTypes)ActiveTab)
            {
                default:
                case TabTypes.PACKETS:
                    using (new GUILayout.VerticalScope("Box"))
                    {
                        GUILayout.Label("TODO: In/out-coming packets log", "header");
                    }
                    break;
            }
        }
    }
}
