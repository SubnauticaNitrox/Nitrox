using System.Globalization;
using System.Threading;

namespace NitroxModel.Helper;

public static class CultureManager
{
    public static readonly CultureInfo CultureInfo = new("en-US");

    /// <summary>
    ///     Internal Subnautica files are setup using US english number formats and dates.  To ensure
    ///     that we parse all of these appropriately, we will set the default cultureInfo to en-US.
    ///     This must best done for any thread that is spun up and needs to read from files (unless
    ///     we were to migrate to 4.5.)  Failure to set the context can result in very strange behaviour
    ///     throughout the entire application.  This originally manifested itself as a duplicate spawning
    ///     issue for players in Europe.  This was due to incorrect parsing of probability tables.
    /// </summary>
    public static void ConfigureCultureInfo()
    {
        // Although we loaded the en-US cultureInfo, let's make sure to set these in case the
        // default was overriden by the user.
        CultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.NumberFormat.NumberGroupSeparator = ",";

        Thread.CurrentThread.CurrentCulture = CultureInfo;
        Thread.CurrentThread.CurrentUICulture = CultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo;
    }
}
