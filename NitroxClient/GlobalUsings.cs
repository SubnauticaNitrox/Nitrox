global using NitroxClient.Helpers;
global using NitroxClient.Extensions;
global using Nitrox.Model.Extensions;
global using Nitrox.Model.Logger;
global using Nitrox.Model.Subnautica.Extensions;
global using Task = System.Threading.Tasks.Task;
#if NET
global using LockObject = System.Threading.Lock;
#else
global using LockObject = object;
#endif
