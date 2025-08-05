using System;

namespace Nitrox.Launcher.Models.Attributes;

/// <summary>
///     Marks a type as an HttpService which will make it accessible through dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class HttpServiceAttribute : Attribute;
