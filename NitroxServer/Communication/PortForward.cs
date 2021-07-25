using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mono.Nat;
using NitroxModel.DataStructures;

namespace NitroxServer.Communication
{
    public static class PortForward
    {
        private static event EventHandler<INatDevice> DeviceDiscovered;
        private static readonly List<INatDevice> devices = new();
        private static readonly ThreadSafeDictionary<int, Exception> lastPortErrors = new();

        /// <summary>
        ///     Tries to open the given port.
        /// </summary>
        public static async Task<bool> TryOpenPortAsync(int port, TimeSpan timeout)
        {
            static async Task PortForwardDeviceAsync(IDictionary<int, Exception> errors, INatDevice device, int port)
            {
                try
                {
                    await device.CreatePortMapAsync(new Mapping(Protocol.Udp, port, port, (int)TimeSpan.FromDays(1).TotalSeconds, "Nitrox Server"));
                    errors[port] = null;
                }
                catch (MappingException ex)
                {
                    if (ex.ErrorCode != ErrorCode.ConflictInMappingEntry)
                    {
                        errors[port] = ex;
                    }
                }
            }

            BeginDiscoverDevices();
            List<INatDevice> devicesCopy;
            lock (devices)
            {
                devicesCopy = new List<INatDevice>(devices);
            }
            DeviceDiscovered += async (sender, device) =>
            {
                await PortForwardDeviceAsync(lastPortErrors, device, port);
            };
            foreach (INatDevice device in devicesCopy)
            {
                await PortForwardDeviceAsync(lastPortErrors, device, port);
            }
            return await IsPortOpenAsync(port, timeout);
        }

        public static string GetError(int port)
        {
            if (lastPortErrors.TryGetValue(port, out Exception ex))
            {
                return ex.Message;
            }
            
            return null;
        }
        
        public static async Task<bool> IsPortOpenAsync(int port, TimeSpan timeout)
        {
            CancellationTokenSource source = new(timeout);
            while (!source.IsCancellationRequested)
            {
                if (lastPortErrors.TryGetValue(port, out Exception ex))
                {
                    if (ex == null)
                    {
                        return true;
                    }
                }
                else
                {
                    try
                    {
                        await Task.Delay(50, source.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // ignored
                    }
                }
            }
            return false;
        }

        private static void BeginDiscoverDevices()
        {
            if (NatUtility.IsSearching)
            {
                return;
            }
            NatUtility.DeviceFound += (_, args) =>
            {
                lock (devices)
                {
                    OnDeviceDiscovered(args.Device);
                    devices.Add(args.Device);
                }
            };
            NatUtility.StartDiscovery();
        }

        private static void OnDeviceDiscovered(INatDevice e)
        {
            DeviceDiscovered?.Invoke(null, e);
        }
    }
}
