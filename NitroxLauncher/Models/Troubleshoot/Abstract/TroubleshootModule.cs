using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NitroxModel;

namespace NitroxLauncher.Models.Troubleshoot.Abstract
{
    public abstract class TroubleshootModule : INotifyPropertyChanged
    {
        public string StatusCode => Status.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";

        private TroubleshootStatus status;
        public TroubleshootStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusCode));
            }
        }

        public string Name { get; }

        protected TroubleshootModule(string name)
        {
            Status = TroubleshootStatus.NOT_STARTED;
            Name = name;
        }

        public void RunDiagnostic()
        {
            Status = TroubleshootStatus.RUNNING;

            try
            {
                Check();
                Status = TroubleshootStatus.OK;
            }
            catch (Exception)
            {
                LauncherNotifier.Error("Troubleshoot check stopped working");
                Status = TroubleshootStatus.FATAL_ERROR;
            }
        }

        protected abstract bool? Check();

        #region ------ PropertyChanged -------
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
