using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NitroxClient.Helpers
{
    internal class WaitUntilTrueOrTime : CustomYieldInstruction
    {
        private float waitUntilTime = -1f;
        private Func<bool> predicate;
        private float waitTime;

        public bool waitForTime
        {
            get
            {
                if (waitUntilTime < 0f)
                {
                    waitUntilTime = Time.realtimeSinceStartup + waitTime;
                }

                bool flag = Time.realtimeSinceStartup < waitUntilTime;
                if (!flag)
                {
                    waitUntilTime = -1f;
                }

                return flag;
            }
        }

        public override bool keepWaiting => !predicate() && waitForTime;
        public WaitUntilTrueOrTime(float timeInSeconds, Func<bool> predicate)
        {
            this.predicate = predicate;
            this.waitTime = timeInSeconds;
        }
    }
}
