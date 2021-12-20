using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxLauncher.Install.Core;

public sealed record InstallResult
{
    public static readonly InstallResult OkResult = new();
    public static readonly InstallResult AuthErrorResult = new()
    {
        AuthorizationRequired = true,
        Message = "Unknown authorization error occurred",
        InnerException = new UnauthorizedAccessException()
    };
    public static readonly InstallResult ErrorResult = new() { InnerException = new Exception("Unknown install error occurred") };

    private readonly string message;
    public bool AuthorizationRequired { get; init; }

    public string Message
    {
        get => message ?? InnerException?.Message;
        init => message = value;
    }

    public Exception InnerException { get; init; }
    public bool Success => InnerException == null;
    public object Origin { get; init; }

    public static InstallResult From(Exception exception)
    {
        return new InstallResult { InnerException = exception, AuthorizationRequired = exception is UnauthorizedAccessException };
    }

    public static implicit operator InstallResult(bool success)
    {
        return success ? OkResult : ErrorResult;
    }

    public static implicit operator InstallResult(Exception error)
    {
        return From(error);
    }

    public static implicit operator InstallResult(string error)
    {
        return From(new Exception(error));
    }

    public static string GetPrettyErrorMessage(IEnumerable<InstallResult> results, string message)
    {
        IEnumerable<string> items = results.Where(r => !r.Success).Select(r => $"{r.Origin.GetType().Name}: {r.Message}").ToArray();
        if (!items.Any())
        {
            return null;
        }
        return $"{message}{Environment.NewLine}{Environment.NewLine} - {string.Join(" - ", items)}";
    }
}
