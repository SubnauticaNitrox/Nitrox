namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

internal interface IArgConverter
{
    Task<ConvertResult> ConvertAsync(object from);
}

/// <summary>
///     Converts an object of type <see cref="TFrom" /> to <see cref="TTo" />.
/// </summary>
internal interface IArgConverter<in TFrom, TTo> : IArgConverter
{
    Task<ConvertResult> ConvertAsync(TFrom from);

    Task<ConvertResult> IArgConverter.ConvertAsync(object from) => ConvertAsync(from is TFrom tFrom ? tFrom : default);
}
