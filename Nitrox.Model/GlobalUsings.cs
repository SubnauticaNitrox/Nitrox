#if NET
global using LockObject = System.Threading.Lock;
#else
global using LockObject = object;
#endif
global using System.Runtime.CompilerServices;
global using Nitrox.Model.Extensions;
