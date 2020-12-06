﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.OS;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal sealed class ConfigCommand : Command
    {
        private readonly SemaphoreSlim configOpenLock = new SemaphoreSlim(1);

        public ConfigCommand() : base("config", Perms.CONSOLE, "Opens the server configuration file")
        {
        }

        protected override void Execute(CallArgs args)
        {
            if (!configOpenLock.Wait(0))
            {
                Log.Warn("Waiting on previous config command to close the configuration file.");
                return;
            }

            ServerConfig currentActiveConfig = NitroxServiceLocator.LocateService<ServerConfig>();
            string configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", currentActiveConfig.FileName);
            if (!File.Exists(configFile))
            {
                Log.Error($"Could not find config file at: {configFile}");
                return;
            }

            Task.Run(async () =>
                {
                    try
                    {
                        await StartWithDefaultProgram(configFile);
                    }
                    finally
                    {
                        configOpenLock.Release();
                    }
                    PropertiesWriter.Deserialize<ServerConfig>(); // Notifies user if deserialization failed.
                    Log.Info("If you made changes, restart the server for them to take effect.");
                })
                .ContinueWith(t =>
                {
#if DEBUG
                    if (t.Exception != null)
                    {
                        throw t.Exception;
                    }
#endif
                });
        }

        private async Task StartWithDefaultProgram(string fileToOpen)
        {
            using Process process = FileSystem.Instance.OpenOrExecuteFile(fileToOpen);
            process.WaitForExit();
            try
            {
                while (!process.HasExited)
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
