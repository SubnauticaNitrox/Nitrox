using NitroxModel.Helper;

namespace NitroxModel.Logger
{
    /// <summary>
    ///     Static logger for projects that do not have Dependency Injection.
    /// </summary>
    public static class StaticLogger
    {
        private static INitroxLogger instance;

        /// <summary>
        ///     Gets or sets the instance that is used for logging. Defaults to <see cref="NoLogger.Default" />.
        /// </summary>
        public static INitroxLogger Instance
        {
            get { return instance; }
            set
            {
                Validate.NotNull(value);
                instance = value;
            }
        }

        static StaticLogger()
        {
            instance = NoLogger.Default;
        }
    }
}
