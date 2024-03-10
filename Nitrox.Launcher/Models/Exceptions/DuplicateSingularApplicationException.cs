using System;

namespace Nitrox.Launcher.Models.Exceptions;

public class DuplicateSingularApplicationException : Exception
{
    public DuplicateSingularApplicationException(string applicationName) : base($"An instance of {applicationName} is already running")
    {
    }
}
