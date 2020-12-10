using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using NitroxModel.Helper;

namespace NitroxServer_Subnautica
{
    public static class AppMutex
    {
        private static readonly SemaphoreSlim mutexReleaseGate = new SemaphoreSlim(1);
        private static readonly SemaphoreSlim callerGate = new SemaphoreSlim(1);

        public static void Hold(Action onWaitingForMutex = null, int timeoutInMs = 5000)
        {
            Validate.IsTrue(timeoutInMs >= 5000, "Timeout must be at least 5 seconds.");

            using CancellationTokenSource acquireSource = new CancellationTokenSource(timeoutInMs);
            CancellationToken token = acquireSource.Token;
            Thread thread = new Thread(o =>
            {
                bool first = true;
                string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
                string mutexId = $@"Global\{{{appGuid}}}";
                MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                                                        MutexRights.FullControl,
                                                                        AccessControlType.Allow
                );
                MutexSecurity securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);

                Mutex mutex = new Mutex(false, mutexId, out bool _, securitySettings);
                try
                {
                    try
                    {
                        while (!mutex.WaitOne(100, false))
                        {
                            token.ThrowIfCancellationRequested();
                            if (first)
                            {
                                first = false;
                                onWaitingForMutex?.Invoke();
                            }
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Mutex was abandoned in another process, it will still get acquired
                    }
                }
                finally
                {
                    callerGate.Release();
                    mutexReleaseGate.Wait(-1);
                    mutex.ReleaseMutex();
                }
            });
            mutexReleaseGate.Wait(-1, token);
            callerGate.Wait(0, token);
            thread.Start();

            while (!callerGate.Wait(100, token))
            {
            }
        }

        public static void Release()
        {
            mutexReleaseGate.Release();
        }
    }
}
