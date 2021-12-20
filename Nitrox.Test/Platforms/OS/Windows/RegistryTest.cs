using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxTest.Platforms.OS.Windows
{
    [TestClass]
    public class RegistryTest
    {
        [TestMethod]
        public async Task WaitsForRegistryKeyToExist()
        {
            const string pathToKey = @"SOFTWARE\Nitrox\test";
            
            RegistryEx.Write(pathToKey, 0);
            var readTask = Task.Run(async () =>
            {
                try
                {
                    await RegistryEx.CompareAsync<int>(pathToKey,
                                                       v => v == 1337,
                                                       TimeSpan.FromSeconds(5));
                    return true;
                }
                catch (TaskCanceledException)
                {
                    return false;
                }
            });
            
            RegistryEx.Write(pathToKey, 1337);
            Assert.IsTrue(await readTask);
            
            // Cleanup (we can keep "Nitrox" key intact).
            RegistryEx.Delete(pathToKey);
            Assert.IsNull(RegistryEx.Read<string>(pathToKey));
        }
    }
}