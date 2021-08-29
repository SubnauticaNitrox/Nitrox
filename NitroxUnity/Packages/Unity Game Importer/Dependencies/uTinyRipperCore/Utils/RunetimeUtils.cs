using System;
using System.Runtime.InteropServices;

namespace uTinyRipper
{
	public static class RunetimeUtils
	{
		static RunetimeUtils()
		{
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
			IsRunningOnNetCore = false;
			//RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.Ordinal);
		}

		public static bool IsRunningOnMono { get; }
		public static bool IsRunningOnNetCore { get; }
	}
}
