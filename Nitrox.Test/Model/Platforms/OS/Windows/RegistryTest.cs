using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nitrox.Test.Model.Platforms;

namespace NitroxModel.Platforms.OS.Windows;

[TestClass]
public class RegistryTest
{
    [OSTestMethod("WINDOWS")]
    public async Task WaitsForRegistryKeyToExist()
    {
        const string PATH_TO_KEY = @"SOFTWARE\Nitrox\test";
        
        RegistryEx.Write(PATH_TO_KEY, 0);
        Task<bool> readTask = Task.Run(async () =>
        {
            try
            {
                await RegistryEx.CompareAsync<int>(PATH_TO_KEY,
                                                   v => v == 1337,
                                                   TimeSpan.FromSeconds(5));
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        });
        
        RegistryEx.Write(PATH_TO_KEY, 1337);
        Assert.IsTrue(await readTask);
        
        // Cleanup (we can keep "Nitrox" key intact).
        RegistryEx.Delete(PATH_TO_KEY);
        Assert.IsNull(RegistryEx.Read<string>(PATH_TO_KEY));
    }
}
