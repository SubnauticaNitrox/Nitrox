using System.Reflection;
using Nitrox.Model;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets.Core;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.Debuggers;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.GameLogic.Settings;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;

namespace NitroxClient;


// TODO: IMPLEMENT AS MSDI!

// internal sealed class ClientAutoFacRegistrar
// {
//     // public void RegisterDependencies(ContainerBuilder containerBuilder)
//     // {
//     //     RegisterCoreDependencies(containerBuilder);
//     //     RegisterMetadataDependencies(containerBuilder);
//     //     RegisterPacketProcessors(containerBuilder);
//     //     RegisterColorSwapManagers(containerBuilder);
//     //     RegisterInitialSyncProcessors(containerBuilder);
//     // }
//
//     private void RegisterCoreDependencies(ContainerBuilder containerBuilder)
//     {
// // #if DEBUG
// //         containerBuilder.RegisterAssemblyTypes(currentAssembly)
// //                         .AssignableTo<AbstractDebugger>()
// //                         .As<AbstractDebugger>()
// //                         .AsImplementedInterfaces()
// //                         .AsSelf()
// //                         .SingleInstance();
// // #endif
//         // containerBuilder.Register(c => new NitroxProtobufSerializer($"{nameof(Nitrox)}.{nameof(Nitrox.Model)}.dll"));
//
//         containerBuilder.RegisterType<UnityPreferenceStateProvider>()
//                         .As<IPreferenceStateProvider>()
//                         .SingleInstance();
//
//         containerBuilder.RegisterType<PlayerPreferenceManager>().SingleInstance();
//
//         containerBuilder.RegisterType<MultiplayerSessionManager>()
//                         .As<IMultiplayerSession>()
//                         .As<IPacketSender>()
//                         .InstancePerLifetimeScope();
//
//         containerBuilder.RegisterType<LiteNetLibClient>()
//                         .As<IClient>()
//                         .InstancePerLifetimeScope();
//
//         containerBuilder.RegisterType<LocalPlayer>()
//                         .AsSelf() //Would like to deprecate this registration at some point and just work through an abstraction.
//                         .As<ILocalNitroxPlayer>()
//                         .InstancePerLifetimeScope();
//
//         containerBuilder.RegisterType<PlayerManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<PlayerModelManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<PlayerVitalsManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<PacketReceiver>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Vehicles>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<AI>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<SimulationOwnership>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<LiveMixinManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Entities>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<MedkitFabricator>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Items>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<EquipmentSlots>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<ItemContainers>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Cyclops>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Rockets>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<MobileVehicleBay>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Interior>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<NitroxConsole>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Terrain>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<ExosuitModuleEvent>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<SeamothModulesEvent>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<Fires>().InstancePerLifetimeScope();
//         containerBuilder.Register(_ => FMODWhitelist.Load(GameInfo.Subnautica)).InstancePerLifetimeScope();
//         containerBuilder.RegisterType<FMODSystem>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<NitroxSettingsManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<ThrottledPacketSender>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<PlayerCinematics>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<NitroxPDATabManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<TimeManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<SleepManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<BulletManager>().InstancePerLifetimeScope();
//         containerBuilder.RegisterType<NtpSyncer>().InstancePerLifetimeScope();
//     }
//
//     private void RegisterMetadataDependencies(ContainerBuilder containerBuilder)
//     {
//         containerBuilder.RegisterAssemblyTypes(currentAssembly)
//                         .AssignableTo<IEntityMetadataExtractor>()
//                         .As<IEntityMetadataExtractor>()
//                         .AsSelf()
//                         .SingleInstance();
//         containerBuilder.RegisterAssemblyTypes(currentAssembly)
//                         .AssignableTo<IEntityMetadataProcessor>()
//                         .As<IEntityMetadataProcessor>()
//                         .AsSelf()
//                         .SingleInstance();
//         containerBuilder.RegisterType<EntityMetadataManager>().InstancePerLifetimeScope();
//     }
//
//     private void RegisterPacketProcessors(ContainerBuilder containerBuilder)
//     {
//         containerBuilder
//             .RegisterAssemblyTypes(currentAssembly)
//             .AsClosedTypesOf(typeof(IClientPacketProcessor<>))
//             .As<IPacketProcessor>()
//             .InstancePerLifetimeScope();
//
//         containerBuilder.RegisterType<PacketProcessorsInvoker>().InstancePerLifetimeScope();
//     }
//
//     private void RegisterColorSwapManagers(ContainerBuilder containerBuilder)
//     {
//         containerBuilder
//             .RegisterAssemblyTypes(currentAssembly)
//             .AssignableTo<IColorSwapManager>()
//             .As<IColorSwapManager>()
//             .SingleInstance();
//     }
//
//     private void RegisterInitialSyncProcessors(ContainerBuilder containerBuilder)
//     {
//         containerBuilder
//             .RegisterAssemblyTypes(currentAssembly)
//             .AssignableTo<IInitialSyncProcessor>()
//             .As<IInitialSyncProcessor>()
//             .InstancePerLifetimeScope();
//     }
// }
