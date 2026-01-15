#if MODERN_DOTNET
global using LockObject = System.Threading.Lock;
#else
global using LockObject = object;
#endif
global using Nitrox.Model.Extensions;
