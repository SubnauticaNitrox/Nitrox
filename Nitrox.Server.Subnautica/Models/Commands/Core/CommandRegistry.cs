using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

/// <summary>
///     Aggregates known commands into a read-optimized and sorted lookup.
/// </summary>
internal sealed class CommandRegistry
{
    private static readonly Type[] specificToGeneralizingTypeOrder =
        [typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(object), typeof(string)];

    private readonly ILogger<CommandRegistry> logger;

    /// <summary>
    ///     Lookup for command name -> list of known handlers. Each with different arg types or count.
    /// </summary>
    private Dictionary<string, List<CommandHandlerEntry>> HandlerLookup { get; } = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, List<CommandHandlerEntry>>.AlternateLookup<ReadOnlySpan<char>> SpanHandlerLookup { get; }

    /// <summary>
    ///     Lookup of converters that can convert to a type.
    /// </summary>
    private Dictionary<Type, List<ArgConverterInfo>> ArgConverterLookup { get; } = [];

    public IEnumerable<CommandHandlerEntry> Handlers => HandlerLookup
                                                        .DistinctBy(p => p.Value)
                                                        .SelectMany(p => p.Value);

    public CommandRegistry(IEnumerable<CommandHandlerEntry> handlers, IEnumerable<IArgConverter> argConverters, ILogger<CommandRegistry> logger)
    {
        this.logger = logger;
        CommandHandlerEntry[] commandHandlers = [.. handlers];
        foreach (CommandHandlerEntry handler in commandHandlers
                                                .SelectMany(h =>
                                                {
                                                    // Split handlers with optional parameters into separate handlers.
                                                    List<CommandHandlerEntry> result = [h];
                                                    List<object> defaultValues = [];
                                                    for (int i = h.Parameters.Length - 1; i >= 0; i--)
                                                    {
                                                        ParameterInfo current = h.Parameters[i];
                                                        if (current.IsOptional)
                                                        {
                                                            defaultValues.Insert(0, current.DefaultValue);
                                                            result.Add(new CommandHandlerEntry(h, h.Parameters.Take(i).ToArray(), [..defaultValues]));
                                                        }
                                                    }
                                                    return result;
                                                })
                                                .OrderByDescending(h => h.Parameters.Length)
                                                .ThenBy(h => h.ParameterTypes.Length == 0
                                                            ? 0
                                                            : h.ParameterTypes.Max(t =>
                                                            {
                                                                // More specific handler parameters should be prioritized over generalizing handlers (i.e. try handlers in this order: bool -> int -> float -> Player -> object -> string).
                                                                int index = specificToGeneralizingTypeOrder.GetIndex(t);
                                                                // String is a catch-all type and comes last. This is because command input is string-based. Anything assignable to object but not string comes before string.
                                                                return index == -1 ? specificToGeneralizingTypeOrder.GetIndex(typeof(object)) : index;
                                                            }))
                                                .ThenBy(h => h.ParameterTypes.Any(o => o == typeof(object)) ? 1 : 0))
        {
            RegisterHandler(handler);
        }
        SpanHandlerLookup = HandlerLookup.GetAlternateLookup<ReadOnlySpan<char>>();
        logger.ZLogDebug($"{commandHandlers.Length:@CommandCount} commands found and registered");

        IArgConverter[] converters = [.. argConverters];
        foreach (IArgConverter converter in converters)
        {
            Type[] converterInterfaces = converter.GetType().GetInterfaces();
            foreach (Type converterInterface in converterInterfaces)
            {
                if (!converterInterface.IsAssignableTo(typeof(IArgConverter)))
                {
                    continue;
                }
                Type[] genericArgsOnConverter = converterInterface.GetGenericArguments();
                if (genericArgsOnConverter is not { Length: 2 })
                {
                    continue;
                }
                Type toType = genericArgsOnConverter[1];
                if (!ArgConverterLookup.TryGetValue(toType, out List<ArgConverterInfo> registeredConverters))
                {
                    registeredConverters = [];
                }
                registeredConverters.Add(new ArgConverterInfo(converter, genericArgsOnConverter[0], toType));
                ArgConverterLookup[toType] = registeredConverters;
            }
        }
    }

    /// <summary>
    ///     Gets the command handlers if command exists. Returns false if context origin (or permissions) are not
    ///     compatible/allowed to execute any of the handlers.
    /// </summary>
    public bool TryGetHandlersByCommandName(ICommandContext context, ReadOnlySpan<char> commandNameOrAlias, out List<CommandHandlerEntry> validHandlers)
    {
        validHandlers = [];
        if (!SpanHandlerLookup.TryGetValue(commandNameOrAlias, out List<CommandHandlerEntry> knownHandlers))
        {
            return false;
        }
        foreach (CommandHandlerEntry handler in knownHandlers)
        {
            if (!IsValidHandlerForContext(handler, context))
            {
                continue;
            }
            validHandlers.Add(handler);
        }
        return validHandlers.Count > 0;
    }

    public bool IsValidHandlerForContext(CommandHandlerEntry handler, ICommandContext context) => handler.AcceptedOrigin.HasFlag(context.Origin) && handler.MinimumPermissions <= context.Permissions;

    public async ValueTask<List<ConvertResult>> TryConvertToType(string value, Type targetType)
    {
        List<ConvertResult> results = [];
        if (ArgConverterLookup.TryGetValue(targetType, out List<ArgConverterInfo> list))
        {
            foreach (ArgConverterInfo converterInfo in list)
            {
                if (TryParseToType(value, converterInfo.From) is not { } obj)
                {
                    continue;
                }
                ConvertResult result = await converterInfo.Converter.ConvertAsync(obj);
                if (result.Success)
                {
                    return [result];
                }
                results.Add(result);
            }
        }
        return results;
    }

    public object? TryParseToType(ReadOnlySpan<char> value, Type type)
    {
        try
        {
            switch (type)
            {
                case null:
                    throw new ArgumentNullException(nameof(type));
                case not null when type == typeof(string):
                    return value.ToString();
                case not null when type == typeof(bool):
                    return bool.Parse(value);
                case not null when type == typeof(char):
                    return char.Parse(value.ToString());
                case not null when type == typeof(sbyte):
                    return sbyte.Parse(value);
                case not null when type == typeof(byte):
                    return byte.Parse(value);
                case not null when type == typeof(short):
                    return short.Parse(value);
                case not null when type == typeof(ushort):
                    return ushort.Parse(value);
                case not null when type == typeof(int):
                    return int.Parse(value);
                case not null when type == typeof(uint):
                    return uint.Parse(value);
                case not null when type == typeof(long):
                    return long.Parse(value);
                case not null when type == typeof(ulong):
                    return ulong.Parse(value);
                case not null when type == typeof(float):
                    return float.Parse(value);
                case not null when type == typeof(double):
                    return double.Parse(value);
                case { IsEnum: true }:
                    return Enum.Parse(type, value, true);
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

    public string? FindCommandName(Regex regex, Perms viewerPerms, bool ignoreAliases = false)
    {
        foreach ((string commandName, List<CommandHandlerEntry>? handlers) in HandlerLookup)
        {
            if (handlers == null)
            {
                continue;
            }
            if (!handlers[0].AcceptedOrigin.HasFlag(CommandOrigin.PLAYER))
            {
                continue;
            }
            bool canViewCommand = false;
            foreach (CommandHandlerEntry h in handlers)
            {
                if (h.MinimumPermissions <= viewerPerms)
                {
                    canViewCommand = true;
                    break;
                }
            }
            if (!canViewCommand)
            {
                continue;
            }
            if (ignoreAliases && handlers[0].Aliases.Contains(commandName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }
            if (!regex.IsMatch(commandName))
            {
                continue;
            }

            return commandName;
        }
        return null;
    }

    private void RegisterHandler(CommandHandlerEntry handler)
    {
        logger.LogCommandHandlerAdded(handler);

        string ownerName = handler.Name;
        if (!HandlerLookup.TryGetValue(ownerName, out List<CommandHandlerEntry> handlers))
        {
            HandlerLookup[ownerName] = handlers = [];
            foreach (string ownerAlias in handler.Aliases)
            {
                if (HandlerLookup.TryGetValue(ownerAlias, out List<CommandHandlerEntry> existingHandlers))
                {
                    if (existingHandlers.Count < 1)
                    {
                        throw new Exception($"Alias {ownerAlias} from {handler.Owner.GetType().FullName} conflicts with its own name");
                    }
                    throw new Exception($"Alias {ownerAlias} from {handler.Owner.GetType().FullName} conflicts with {existingHandlers.First().Owner.GetType().FullName}");
                }
                HandlerLookup[ownerAlias] = handlers;
            }
        }

        if (handlers.Count > 0)
        {
            CommandHandlerEntry otherHandler = handlers.First();
            if (otherHandler.Owner != handler.Owner)
            {
                throw new Exception($"Handler {otherHandler.Name} on type {otherHandler.Owner.GetType().FullName} conflicts with command name {handler.Owner.GetType().FullName}");
            }
        }
        handlers.Add(handler);
    }

    private record ArgConverterInfo(IArgConverter Converter, Type From, Type To);
}
