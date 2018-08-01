using System.ComponentModel;
using System.Collections;
using System.Configuration.Install;

namespace InstallerActions
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        private NitroxInstallPatcher Patcher => new NitroxInstallPatcher(Context.Parameters["baseInstallDirectory"].TrimEnd('\\'));

        public Installer()
        {
            System.Diagnostics.Debugger.Launch();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            if (!Patcher.RequiredAssembliesExist())
            {
                throw new InstallException($"Error instaliing Nitrox to the specified directory. Please ensure the installer is pointing to your subnautica directory and try again.");
            }

            base.OnBeforeInstall(savedState);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            Patcher.InstallPatches();
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            Patcher.RemovePatches();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            Patcher.RemovePatches();
        }
    }
}
