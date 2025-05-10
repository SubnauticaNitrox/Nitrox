using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

public record CommandHandlerEntry
{
    private readonly MethodInvoker execute;
    public object Owner { get; }
    public string Name { get; } = "";
    public MethodInfo ExecuteMethod { get; }
    public ParameterInfo[] Parameters { get; } = [];
    public Type[] ParameterTypes { get; } = [];
    public string[] Aliases { get; init; } = [];
    public string Description { get; init; } = "";

    /// <inheritdoc cref="RequiresOrigin.AcceptedOrigin" />
    public CommandOrigin AcceptedOrigin { get; init; }

    /// <inheritdoc cref="RequiresPermissionAttribute.MinimumPermission" />
    public Perms MinimumPermissions { get; init; }

    public CommandHandlerEntry(ICommandHandlerBase owner, Type handlerType)
    {
        Type ownerType = owner.GetType();
        MethodInfo[] methods = handlerType == null ? ownerType.GetMethods() : ownerType.GetInterfaceMap(handlerType).TargetMethods;
        MethodInfo executeMethod = methods.First(m => m.Name == nameof(ICommandHandler.Execute));

        Owner = owner;
        Name = GetName(owner);
        Description = executeMethod.GetCustomAttribute<DescriptionAttribute>()?.Description;
        ExecuteMethod = executeMethod;
        Parameters = executeMethod.GetParameters().Skip(1).ToArray();
        ParameterTypes = Parameters.Select(p => p.ParameterType).ToArray();
        Type unsupportedType = ParameterTypes.FirstOrDefault(t => t.IsArray || typeof(IList).IsAssignableFrom(t));
        if (unsupportedType != null)
        {
            throw new NotSupportedException($"Arrays like {unsupportedType} are unsupported");
        }
        Aliases = ownerType.GetCustomAttribute<AliasAttribute>()?.Aliases ?? [];
        AcceptedOrigin = executeMethod.GetCustomAttribute<RequiresOrigin>()?.AcceptedOrigin ?? ownerType.GetCustomAttribute<RequiresOrigin>()?.AcceptedOrigin ?? CommandOrigin.DEFAULT;
        MinimumPermissions = executeMethod.GetCustomAttribute<RequiresPermissionAttribute>()?.MinimumPermission ?? ownerType.GetCustomAttribute<RequiresPermissionAttribute>()?.MinimumPermission ?? Perms.DEFAULT;

        execute = MethodInvoker.Create(executeMethod);

        // This asks the JIT to compile this method which reduces initial command execution time.
        RuntimeHelpers.PrepareMethod(executeMethod.MethodHandle);
    }

    public CommandHandlerEntry(CommandHandlerEntry derivedHandler, ParameterInfo[] parameters)
    {
        Owner = derivedHandler.Owner;
        Name = derivedHandler.Name;
        Description = derivedHandler.Description;
        ExecuteMethod = derivedHandler.ExecuteMethod;
        Parameters = parameters;
        ParameterTypes = Parameters.Select(p => p.ParameterType).ToArray();

        execute = derivedHandler.execute;
    }

    public Task InvokeAsync(params Span<object> args) => (Task)execute.Invoke(Owner, args);

    public override string ToString() => ToDisplayString(true);

    public string ToDisplayString(bool includeNames)
    {
        string nameText = "";
        if (includeNames)
        {
            nameText = string.Join('|', Aliases.OrderBy(n => n.Length).ThenBy(n => n));
            if (nameText != "")
            {
                nameText = $"|{nameText}";
            }
            nameText = $"{Name}{nameText} ";
        }

        return $"{nameText}{GetParametersInfo(Parameters)}- {Description}";

        static string GetParametersInfo(params ParameterInfo[] parms)
        {
            if (parms.Length < 1)
            {
                return "";
            }

            return $"{string.Join(" ", parms.Select(GetParameterInfo))} ";

            static string GetParameterInfo(ParameterInfo parameter)
            {
                string surrounding = parameter.IsOptional ? "[]" : "<>";
                return $"{surrounding[0]}{parameter.Name}:{GetNiceTypeName(parameter.ParameterType)}{surrounding[1]}";
            }

            static string GetNiceTypeName(Type type)
            {
                if (type.IsEnum)
                {
                    return $"'{string.Join("'|'", Enum.GetNames(type))}'";
                }
                if (type == typeof(int))
                {
                    return "int";
                }
                if (type == typeof(string))
                {
                    return "string";
                }
                if (type == typeof(float))
                {
                    return "float";
                }
                if (type == typeof(object))
                {
                    return "object";
                }
                return type.Name;
            }
        }
    }

    private static string GetName(object owner)
    {
        string name = owner.GetType().Name;
        int lastIndexOf = name.LastIndexOf("Command", StringComparison.Ordinal);
        if (lastIndexOf == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(owner), @"Expected command type name to end with ""Command""");
        }
        return name[.. lastIndexOf].ToLowerInvariant();
    }
}
