using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Mono.Nat;

namespace NitroxModel.Helper;

public static class NatHelper
{
    public static async Task<IPAddress> GetExternalIpAsync()
    {
        return await MonoNatHelper.GetFirstAsync(async d => await d.GetExternalIPAsync().ConfigureAwait(false)).ConfigureAwait(false);
    }

    public static async Task<bool> DeletePortMappingAsync(ushort port, Protocol protocol)
    {
        Mapping mapping = new(protocol, port, port);
        return await MonoNatHelper.GetFirstAsync(async d =>
        {
            try
            {
                return await d.DeletePortMapAsync(mapping).ConfigureAwait(false) != null;
            }
            catch (MappingException)
            {
                return false;
            }
        }).ConfigureAwait(false);
    }

    public static async Task<Mapping> GetPortMappingAsync(ushort port, Protocol protocol)
    {
        return await MonoNatHelper.GetFirstAsync(async d =>
        {
            try
            {
                return await d.GetSpecificMappingAsync(protocol, port).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }).ConfigureAwait(false);
    }

    public static async Task<bool> AddPortMappingAsync(ushort port, Protocol protocol)
    {
        Mapping mapping = new(protocol, port, port);
        return await MonoNatHelper.GetFirstAsync(async d => await d.CreatePortMapAsync(mapping).ConfigureAwait(false) != null).ConfigureAwait(false);
    }

    private static class MonoNatHelper
    {
        private static readonly ConcurrentDictionary<EndPoint, INatDevice> discoveredDevices = new();
        private static readonly object discoverTaskLocker = new();
        private static Task<IEnumerable<INatDevice>> discoverTaskCache;

        public static Task<IEnumerable<INatDevice>> DiscoverAsync()
        {
            // Singleton discovery task. Same task is reused to cache the result of the discovery broadcast.
            lock (discoverTaskLocker)
            {
                if (discoverTaskCache != null)
                {
                    return discoverTaskCache;
                }

                return discoverTaskCache = DiscoveryUncachedAsync(60000, 5000);
            }
        }

        private static DateTime lastFoundDeviceTime;
        private static readonly object lastFoundDeviceTimeLock = new();
        private static async Task<IEnumerable<INatDevice>> DiscoveryUncachedAsync(int timeoutInMs, int timeoutNoMoreDevicesMs)
        {
            void Handler(object sender, DeviceEventArgs args)
            {
                lock (lastFoundDeviceTimeLock)
                {
                    lastFoundDeviceTime = DateTime.UtcNow;
                }
                discoveredDevices.TryAdd(args.Device.DeviceEndpoint, args.Device);
            }

            NatUtility.DeviceFound += Handler;
            NatUtility.StartDiscovery();
            try
            {
                CancellationTokenSource cancellation = new(timeoutInMs);

                lock (lastFoundDeviceTimeLock)
                {
                    lastFoundDeviceTime = DateTime.UtcNow;
                }
                bool hasFoundDeviceRecently = true;
                while (!cancellation.IsCancellationRequested && hasFoundDeviceRecently)
                {
                    lock (lastFoundDeviceTimeLock)
                    {
                        hasFoundDeviceRecently = (DateTime.UtcNow - lastFoundDeviceTime).TotalMilliseconds <= timeoutNoMoreDevicesMs;
                    }

                    await Task.Delay(10, cancellation.Token).ConfigureAwait(false);
                }
            }
            finally
            {
                NatUtility.StopDiscovery();
                NatUtility.DeviceFound -= Handler;
            }

            return discoveredDevices.Values;
        }

        public static async Task<TResult> GetFirstAsync<TResult>(Func<INatDevice, Task<TResult>> predicate)
        {
            // Start NAT discovery (if it hasn't started yet).
            Task<IEnumerable<INatDevice>> discoverTask = DiscoverAsync();
            if (discoverTask.IsCompleted && discoveredDevices.IsEmpty)
            {
                return default;
            }
                
            // Progressively handle devices until first not-null/false result or when discovery times out.
            ConcurrentDictionary<EndPoint, INatDevice> handledDevices = new();
            do
            {
                IEnumerable<KeyValuePair<EndPoint, INatDevice>> unhandledDevices = discoveredDevices.Except(handledDevices).ToArray();
                if (!unhandledDevices.Any())
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }
                                
                foreach (KeyValuePair<EndPoint, INatDevice> pair in unhandledDevices)
                {
                    if (handledDevices.TryAdd(pair.Key, pair.Value))
                    {
                        TResult result = await predicate(pair.Value);
                        if (result is true or not null)
                        {
                            return result;
                        }
                    }
                }
            } while (!discoverTask.IsCompleted);
            
            return default;
        }
    }
}
