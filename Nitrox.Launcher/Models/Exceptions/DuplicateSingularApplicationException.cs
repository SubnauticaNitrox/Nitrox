using System;

namespace Nitrox.Launcher.Models.Exceptions;

public class DuplicateSingularApplicationException(string applicationName) : Exception($"An instance of {applicationName} is already running");
