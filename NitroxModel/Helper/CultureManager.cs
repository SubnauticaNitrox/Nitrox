using System.Globalization;
using System.Threading;

namespace NitroxModel.Helper;

public static class CultureManager
{
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
        CultureInfo cultureInfo = new("en-US");

        // Although we loaded the en-US cultureInfo, let's make sure to set these in case the
        // default was overriden by the user.
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        cultureInfo.NumberFormat.NumberGroupSeparator = ",";

        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
