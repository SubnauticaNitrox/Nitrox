namespace NitroxLauncher.Install.Core;

public interface IInstaller
{
    bool IsInstalled { get; }
    InstallResult Install();
}
