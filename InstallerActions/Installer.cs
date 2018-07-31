using System.ComponentModel;
using System.Collections;
using System.IO;
using System.Configuration.Install;
using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

namespace InstallerActions
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        private const string GameAssemblyName = "Assembly-CSharp.dll";
        private const string NitroxAssemblyName = "NitroxPatcher.dll";
        private const string GameAssemblyModifiedName = "Assembly-CSharp-Nitrox.dll";

        private const string SubnauticaManagedFolderRelativePath = @"\Subnautica_Data\Managed\";

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            System.Diagnostics.Debugger.Launch();
            var assemblyCSharpPath = GetGameAssemblyPath();

            if (!File.Exists(assemblyCSharpPath))
            {
                throw new InstallException("Assembly-CSharp.dll does not exist in install location. Please ensure the installer is pointing to your subnautica directory.");
            }

            base.OnBeforeInstall(savedState);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            InstallNitroxPatcher();
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
            base.Uninstall(savedState);
        }

        private void InstallNitroxPatcher()
        {
            var assemblyCSharpPath = GetGameAssemblyPath();
            var modifiedAssemblyPath = GetModifiedAssemblyPath();

            using (var module = ModuleDefMD.Load(assemblyCSharpPath))
            using (var nitroxPatcherAssembly = ModuleDefMD.Load(assemblyCSharpPath))
            {
                var nitroxMainDefinition = nitroxPatcherAssembly.GetTypes().FirstOrDefault(x => x.Name == "Main");
                var executeMethodDefinition = nitroxMainDefinition.Methods.FirstOrDefault(x => x.Name == "Execute");

                var executeMethodReference = module.Import(executeMethodDefinition);

                var gameInputType = module.GetTypes().First(x => x.FullName == "GameInput");
                var awakeMethod = gameInputType.Methods.First(x => x.Name == "Awake");

                var callNitroxExecuteInstruction = OpCodes.Call.ToInstruction(executeMethodReference);

                awakeMethod.Body.Instructions.Insert(0, callNitroxExecuteInstruction);
                module.Write(modifiedAssemblyPath);
            }

            File.Delete(assemblyCSharpPath);
            File.Move(modifiedAssemblyPath, assemblyCSharpPath);
        }

        private string GetModifiedAssemblyPath()
        {
            var baseInstallDirectory = Context.Parameters["BaseInstallDirectory"];

            return baseInstallDirectory + SubnauticaManagedFolderRelativePath + GameAssemblyModifiedName;
        }

        private string GetGameAssemblyPath()
        {
            var baseInstallDirectory = Context.Parameters["BaseInstallDirectory"];

            return baseInstallDirectory + SubnauticaManagedFolderRelativePath + GameAssemblyName;
        }
    }
}
