namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal interface ICommandHandlerBase;

internal interface ICommandHandler : ICommandHandlerBase
{
    Task Execute(ICommandContext context);
}

internal interface ICommandHandler<in TArg1> : ICommandHandlerBase
{
    Task Execute(ICommandContext context, TArg1 arg);
}

internal interface ICommandHandler<in TArg1, in TArg2> : ICommandHandlerBase
{
    Task Execute(ICommandContext context, TArg1 arg1, TArg2 arg2);
}

internal interface ICommandHandler<in TArg1, in TArg2, in TArg3> : ICommandHandlerBase
{
    Task Execute(ICommandContext context, TArg1 arg1, TArg2 arg2, TArg3 arg3);
}

internal interface ICommandHandler<in TArg1, in TArg2, in TArg3, in TArg4> : ICommandHandlerBase
{
    Task Execute(ICommandContext context, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
}
