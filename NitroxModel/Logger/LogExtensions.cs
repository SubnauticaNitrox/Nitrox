namespace NitroxModel.Logger
{
    public static class LogExtensions
    {
        /// <summary>
        ///     Displays a message in-game as well as in the log files.
        /// </summary>
        /// <remarks>
        ///     Log extension for projects that reference the game directly.
        /// </remarks>
        /// <param name="log"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void InGame(this INitroxLogger log, string message, params object[] args)
        {
            // TODO: Move this extension into a true NitroxModel project that doesn't reference the game dll. Make a NitroxModel.Game project for those that do.
            ErrorMessage.AddError(string.Format(message, args));

            log.Info(message, args);
        }
    }
}
