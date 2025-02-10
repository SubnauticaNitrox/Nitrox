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
    public static async Task<IPAddress> GetExternalIpAsync() => await MonoNatHelper.GetFirstAsync(static async device =>
    {
        try
        {
            return await device.GetExternalIPAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }).ConfigureAwait(false);

    public static async Task<bool> DeletePortMappingAsync(ushort port, Protocol protocol, CancellationToken ct = default)
    {
        int tries = 3;
        while (tries-- >= 0)
        {
            if (await TryRemoveAsync(port, protocol, ct))
            {
                return true;
            }
            await Task.Delay(250, ct);
        }
        return false;

        static async Task<bool> TryRemoveAsync(ushort port, Protocol protocol, CancellationToken ct)
        {
            return await MonoNatHelper.GetFirstAsync(static async (device, mapping) =>
            {
                try
                {
                    return await device.DeletePortMapAsync(mapping).ConfigureAwait(false) != null;
                }
                catch (MappingException)
                {
                    return false;
                }
            }, new Mapping(protocol, port, port), ct).ConfigureAwait(false);
        }
    }

    public static async Task<Mapping> GetPortMappingAsync(ushort port, Protocol protocol, CancellationToken ct = default)
    {
        return await MonoNatHelper.GetFirstAsync(static async (device, protocolAndPort) =>
        {
            try
            {
                return await device.GetSpecificMappingAsync(protocolAndPort.protocol, protocolAndPort.port).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }, (port, protocol), ct).ConfigureAwait(false);
    }

    public static async Task<ResultCodes> AddPortMappingAsync(ushort port, Protocol protocol, CancellationToken ct = default)
    {
        Mapping mapping = new(protocol, port, port);
        return await MonoNatHelper.GetFirstAsync(static async (device, mapping) =>
        {
            try
            {
                return await device.CreatePortMapAsync(mapping).ConfigureAwait(false) != null ? ResultCodes.SUCCESS : ResultCodes.UNKNOWN_ERROR;
            }
            catch (MappingException ex)
            {
                return ExceptionToCode(ex);
            }
        }, mapping, ct).ConfigureAwait(false);
    }

    public enum ResultCodes
    {
        SUCCESS,
        CONFLICT_IN_MAPPING_ENTRY,
        UNKNOWN_ERROR
    }

    private static ResultCodes ExceptionToCode(MappingException exception) => exception.ErrorCode switch
    {
        ErrorCode.ConflictInMappingEntry => ResultCodes.CONFLICT_IN_MAPPING_ENTRY,
        _ => ResultCodes.UNKNOWN_ERROR
    };

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

        public static async Task<TResult> GetFirstAsync<TResult>(Func<INatDevice, Task<TResult>> predicate) => await GetFirstAsync(static (device, p) => p(device), predicate);

        public static async Task<TResult> GetFirstAsync<TResult, TExtraParam>(Func<INatDevice, TExtraParam, Task<TResult>> predicate, TExtraParam parameter, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return default;
            }

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
                    try
                    {
                        await Task.Delay(10, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        // ignored
                    }
                    continue;
                }

                foreach (KeyValuePair<EndPoint, INatDevice> pair in unhandledDevices)
                {
                    if (ct.IsCancellationRequested)
                    {
                        return default;
                    }
                    if (handledDevices.TryAdd(pair.Key, pair.Value))
                    {
                        TResult result = await predicate(pair.Value, parameter);
                        if (result is true or not null)
                        {
                            return result;
                        }
                    }
                }
            } while (!ct.IsCancellationRequested && !discoverTask.IsCompleted);

            return default;
        }
    }
}
