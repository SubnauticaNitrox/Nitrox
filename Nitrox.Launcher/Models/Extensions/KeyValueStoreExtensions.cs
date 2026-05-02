using Nitrox.Model.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class KeyValueStoreExtensions
{
    extension(IKeyValueStore self)
    {
        public string GetLaunchArguments(GameInfo gameInfo, string defaultValue = "-vrmode none") => self.GetValue($"{gameInfo.Name}LaunchArguments", defaultValue);
        public void SetLaunchArguments(GameInfo gameInfo, string value) => self.SetValue($"{gameInfo.Name}LaunchArguments", value);
        public bool GetIsLightModeEnabled(bool defaultValue = false) => self.GetValue("IsLightModeEnabled", defaultValue);
        public void SetIsLightModeEnabled(bool value) => self.SetValue("IsLightModeEnabled", value);
        public bool GetIsMultipleGameInstancesAllowed(bool defaultValue = false) => self.GetValue("IsMultipleGameInstancesAllowed", defaultValue);
        public void SetIsMultipleGameInstancesAllowed(bool value) => self.SetValue("IsMultipleGameInstancesAllowed", value);
        public bool GetPreferEmbedded(bool defaultValue = true) => self.GetValue("PreferEmbedded", defaultValue);
        public void SetPreferEmbedded(bool value) => self.SetValue("PreferEmbedded", value);
        public bool GetUseBigPictureMode(bool defaultValue = false) => self.GetValue("UseBigPictureMode", defaultValue);
        public void SetBigPictureMode(bool value) => self.SetValue("UseBigPictureMode", value);
    }
}
