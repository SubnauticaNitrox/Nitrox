using System.ComponentModel;
using System.Collections;
using System.Configuration.Install;
using System.IO;

namespace InstallerActions
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            NitroxInstallPatcher patcher = new NitroxInstallPatcher(Context.Parameters["baseInstallDirectory"].TrimEnd('\\'));

            if (!patcher.RequiredAssembliesExist())
            {
                throw new InstallException($"Error instaliing Nitrox to the specified directory. Please ensure the installer is pointing to your subnautica directory and try again.");
            }

            base.OnBeforeInstall(savedState);
        }

        public override void Install(IDictionary stateSaver)
        {
            #if DEBUG
                System.Diagnostics.Debugger.Launch();
            #endif
            NitroxInstallPatcher patcher = new NitroxInstallPatcher(Context.Parameters["baseInstallDirectory"].TrimEnd('\\'));

            base.Install(stateSaver);
            patcher.InstallPatches();
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            #if DEBUG
                System.Diagnostics.Debugger.Launch();
            #endif
            string installDirectoy = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
            string baseInstallDirectory = installDirectoy.Replace(NitroxInstallPatcher.SUBNAUTICA_MANAGED_FOLDER_RELATIVE_PATH, string.Empty);
            NitroxInstallPatcher patcher = new NitroxInstallPatcher(baseInstallDirectory);

            base.Uninstall(savedState);
            patcher.RemovePatches();
        }
    }
}
