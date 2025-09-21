# Nitrox Copilot Instructions

## Project Overview
Nitrox is a **multiplayer modification for Subnautica**, enabling multiple players to explore the underwater world together. This is a complex game modification project that intercepts and modifies Unity game behavior through Harmony patches.

## Architecture
The project follows a **client-server architecture** with clear separation of concerns:

- **`NitroxClient/`** - Unity client-side logic, handles game state synchronization
- **`NitroxServer/`** - Standalone dedicated server managing world state  
- **`NitroxModel/`** - Shared data structures and networking protocols
- **`NitroxPatcher/`** - Harmony patches that inject multiplayer logic into Subnautica
- **`Nitrox.Launcher/`** - Avalonia UI launcher for managing client/server

## Critical Patterns

### Dependency Injection with AutoFac
All projects use AutoFac for dependency injection. Each project has an `AutoFacRegistrar` class:
```csharp
public class ClientAutoFacRegistrar : IAutoFacRegistrar
{
    public void RegisterDependencies(ContainerBuilder containerBuilder) { }
}
```
- Use `NitroxServiceLocator.LocateService<T>()` to resolve dependencies
- Register services with appropriate lifetimes: `.SingleInstance()`, `.InstancePerLifetimeScope()`
- Always inherit from `IAutoFacRegistrar` for new DI registrations

### Packet-Based Communication
Communication uses a custom packet system over LiteNetLib:
```csharp
[Serializable]
public class MyPacket : Packet
{
    public ushort PlayerId { get; }
    // Constructor and data
}
```
- **Server processors**: Inherit from `AuthenticatedPacketProcessor<T>` or `UnauthenticatedPacketProcessor<T>`
- **Client processors**: Inherit from `ClientPacketProcessor<T>`
- Packets auto-register via AutoFac assembly scanning

### Harmony Patching System
Game modification uses Harmony patches in `NitroxPatcher/Patches/`:
```csharp
public sealed class MyPatch : NitroxPatch, IDynamicPatch // or IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MyClass t) => t.MyMethod());
    
    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD, ((Func<bool>)Prefix).Method);
    }
}
```
- **Dynamic patches**: Applied during multiplayer sessions (unloadable)
- **Persistent patches**: Applied at game startup (always active)
- Use `Reflect.Method()` for type-safe method targeting

## Development Workflow

### Building
```powershell
# Build entire solution
dotnet build

# Run specific project
dotnet run --project NitroxServer
dotnet run --project Nitrox.Launcher
```

### Testing
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test Nitrox.Test
```
- Uses MSTest with FluentAssertions
- Test projects include custom `NitroxAutoFaker<T>` for generating test data
- Setup with `SetupAssemblyInitializer` for environment configuration

### Key Files to Reference
- **`Directory.Build.props`** - Global build settings (C# 13, unsafe blocks enabled)
- **`GlobalUsings.cs`** - Project-specific global usings (ImplicitUsings disabled)
- **`NitroxServiceLocator`** - Central dependency resolution
- **`PacketHandler`** - Server-side packet routing logic
- **`Patcher.cs`** - Harmony patch application lifecycle

## Game Integration Specifics
- **Unity integration**: Client runs within Subnautica's Unity process
- **Game state sync**: Entities, player positions, building construction, etc.
- **ID management**: Uses `NitroxId` (GUID-based) for consistent object tracking
- **World persistence**: Server maintains save files independent of client saves

## Common Tasks
- **Adding packets**: Create packet class + processor classes for client/server
- **Game behavior modification**: Add Harmony patches in appropriate Persistent/Dynamic folders  
- **New client features**: Register in `ClientAutoFacRegistrar`, implement game logic classes
- **Server functionality**: Add to `ServerAutoFacRegistrar`, create corresponding server classes
- **Cross-project changes**: Update shared `NitroxModel` for data structures

Focus on understanding the networking flow and patch system when making changes - these are the core mechanisms that enable multiplayer functionality in a single-player game.