#if NET5_0_OR_GREATER
global using LockObject = System.Threading.Lock;
#else
global using LockObject = object;
#endif
