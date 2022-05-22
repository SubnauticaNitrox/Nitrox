using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NitroxLauncher.Models.Troubleshoot.Abstract.Events;
using NitroxModel;

namespace NitroxLauncher.Models.Troubleshoot.Abstract
{
    public abstract class TroubleshootModule : INotifyPropertyChanged
    {
        public static event StatusChangedEventHandler StatusChangedEvent;
        public static event LogSentEventHandler LogReceivedEvent;

        public string StatusCode => Status.GetAttribute<DescriptionAttribute>()?.Description ?? "Unknown";

        public string Name { get; init; }

        private TroubleshootStatus status = TroubleshootStatus.NOT_STARTED;
        public TroubleshootStatus Status
        {
            get => status;
            set
            {
                EmitStatus(status, status = value);
                OnPropertyChanged(null);
            }
        }

        public void RunDiagnostic()
        {
            Status = TroubleshootStatus.RUNNING;

            try
            {
                Status = Check() ? TroubleshootStatus.OK : TroubleshootStatus.KO;
            }
            catch (Exception ex)
            {
                LauncherNotifier.Error($"{Name} check stopped working");
                Status = TroubleshootStatus.FATAL_ERROR;
                EmitLog(ex.Message);
            }
        }

        public void Reset()
        {
            Status = TroubleshootStatus.NOT_STARTED;
        }

        protected void EmitLog(string message) => LogReceivedEvent?.Invoke(this, new(Name, message));
        protected void EmitLog(object message) => EmitLog(message?.ToString() ?? string.Empty);
        protected void EmitStatus(TroubleshootStatus oldStatus, TroubleshootStatus newStatus) => StatusChangedEvent?.Invoke(this, new(Name, oldStatus, newStatus));

        protected abstract bool Check();

        #region ------ PropertyChanged -------
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
