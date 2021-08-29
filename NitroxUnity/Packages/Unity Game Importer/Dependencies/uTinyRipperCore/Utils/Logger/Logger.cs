namespace uTinyRipper
{
	public static class Logger
	{
		public static void Log(LogType type, LogCategory category, string message, float progress = 0)
        {
            Instance?.Log(type, category, message, progress);
        }

		public static ILogger Instance { get; set; }
	}
}
