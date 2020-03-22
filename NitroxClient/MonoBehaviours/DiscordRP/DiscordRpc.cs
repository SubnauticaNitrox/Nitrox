using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable IDE0008 // Use explicit type
#pragma warning disable IDE1006 // Naming Styles
namespace NitroxClient.MonoBehaviours.DiscordRP
{
    public class DiscordRpc
    {
        [MonoPInvokeCallback(typeof(OnReadyInfo))]
        public static void ReadyCallback(ref DiscordUser connectedUser) { }
        public delegate void OnReadyInfo(ref DiscordUser connectedUser);

        [MonoPInvokeCallback(typeof(OnDisconnectedInfo))]
        public static void DisconnectedCallback(int errorCode, string message) { }
        public delegate void OnDisconnectedInfo(int errorCode, string message);

        [MonoPInvokeCallback(typeof(OnErrorInfo))]
        public static void ErrorCallback(int errorCode, string message) { }
        public delegate void OnErrorInfo(int errorCode, string message);

        [MonoPInvokeCallback(typeof(OnJoinInfo))]
        public static void JoinCallback(string secret) { }
        public delegate void OnJoinInfo(string secret);

        [MonoPInvokeCallback(typeof(OnSpectateInfo))]
        public static void SpectateCallback(string secret) { }
        public delegate void OnSpectateInfo(string secret);

        [MonoPInvokeCallback(typeof(OnRequestInfo))]
        public static void RequestCallback(ref DiscordUser request) { }
        public delegate void OnRequestInfo(ref DiscordUser request);

        public struct EventHandlers
        {
            public OnReadyInfo readyCallback;
            public OnDisconnectedInfo disconnectedCallback;
            public OnErrorInfo errorCallback;
            public OnJoinInfo joinCallback;
            public OnSpectateInfo spectateCallback;
            public OnRequestInfo requestCallback;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RichPresenceStruct
        {
            public IntPtr state; /* max 128 bytes */
            public IntPtr details; /* max 128 bytes */
            public long startTimestamp;
            public long endTimestamp;
            public IntPtr largeImageKey; /* max 32 bytes */
            public IntPtr largeImageText; /* max 128 bytes */
            public IntPtr smallImageKey; /* max 32 bytes */
            public IntPtr smallImageText; /* max 128 bytes */
            public IntPtr partyId; /* max 128 bytes */
            public int partySize;
            public int partyMax;
            public IntPtr matchSecret; /* max 128 bytes */
            public IntPtr joinSecret; /* max 128 bytes */
            public IntPtr spectateSecret; /* max 128 bytes */
            public bool instance;
        }

        [Serializable]
        public struct DiscordUser
        {
            public string userId;
            public string username;
            public string discriminator;
            public string avatar;
        }

        public enum Reply
        {
            NO = 0,
            YES = 1,
            IGNORE = 2
        }

        [DllImport("discord-rpc", EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

        [DllImport("discord-rpc", EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();

        [DllImport("discord-rpc", EntryPoint = "Discord_RunCallbacks", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RunCallbacks();

        [DllImport("discord-rpc", EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UpdatePresenceNative(ref RichPresenceStruct presence);

        [DllImport("discord-rpc", EntryPoint = "Discord_ClearPresence", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ClearPresence();

        [DllImport("discord-rpc", EntryPoint = "Discord_Respond", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Respond(string userId, Reply reply);

        [DllImport("discord-rpc", EntryPoint = "Discord_UpdateHandlers", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateHandlers(ref EventHandlers handlers);

        public static void UpdatePresence(RichPresence presence)
        {
            var presencestruct = presence.GetStruct();
            UpdatePresenceNative(ref presencestruct);
            presence.FreeMem();
        }

        public class RichPresence
        {
            private RichPresenceStruct presence;
            private readonly List<IntPtr> buffers = new List<IntPtr>(10);

            public string state; /* max 128 bytes */
            public string details; /* max 128 bytes */
            public long startTimestamp;
            public long endTimestamp;
            public string largeImageKey; /* max 32 bytes */
            public string largeImageText; /* max 128 bytes */
            public string smallImageKey; /* max 32 bytes */
            public string smallImageText; /* max 128 bytes */
            public string partyId; /* max 128 bytes */
            public int partySize;
            public int partyMax;
            public string matchSecret; /* max 128 bytes */
            public string joinSecret; /* max 128 bytes */
            public string spectateSecret; /* max 128 bytes */
            public bool instance;

            /// <summary>
            /// Get the <see cref="RichPresenceStruct"/> reprensentation of this instance
            /// </summary>
            /// <returns><see cref="RichPresenceStruct"/> reprensentation of this instance</returns>
            internal RichPresenceStruct GetStruct()
            {
                if (buffers.Count > 0)
                {
                    FreeMem();
                }

                presence.state = StrToPtr(state);
                presence.details = StrToPtr(details);
                presence.startTimestamp = startTimestamp;
                presence.endTimestamp = endTimestamp;
                presence.largeImageKey = StrToPtr(largeImageKey);
                presence.largeImageText = StrToPtr(largeImageText);
                presence.smallImageKey = StrToPtr(smallImageKey);
                presence.smallImageText = StrToPtr(smallImageText);
                presence.partyId = StrToPtr(partyId);
                presence.partySize = partySize;
                presence.partyMax = partyMax;
                presence.matchSecret = StrToPtr(matchSecret);
                presence.joinSecret = StrToPtr(joinSecret);
                presence.spectateSecret = StrToPtr(spectateSecret);
                presence.instance = instance;

                return presence;
            }

            /// <summary>
            /// Returns a pointer to a representation of the given string with a size of maxbytes
            /// </summary>
            /// <param name="input">String to convert</param>
            /// <returns>Pointer to the UTF-8 representation of <see cref="input"/></returns>
            private IntPtr StrToPtr(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    return IntPtr.Zero;
                }

                var convbytecnt = Encoding.UTF8.GetByteCount(input);
                var buffer = Marshal.AllocHGlobal(convbytecnt + 1);
                for (int i = 0; i < convbytecnt + 1; i++)
                {
                    Marshal.WriteByte(buffer, i, 0);
                }
                buffers.Add(buffer);
                Marshal.Copy(Encoding.UTF8.GetBytes(input), 0, buffer, convbytecnt);
                return buffer;
            }

            /// <summary>
            /// Convert string to UTF-8 and add null termination
            /// </summary>
            /// <param name="toconv">string to convert</param>
            /// <returns>UTF-8 representation of <see cref="toconv"/> with added null termination</returns>
            private static string StrToUtf8NullTerm(string toconv)
            {
                var str = toconv.Trim();
                var bytes = Encoding.Default.GetBytes(str);
                if (bytes.Length > 0 && bytes[bytes.Length - 1] != 0)
                {
                    str += "\0\0";
                }
                return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(str));
            }

            /// <summary>
            /// Free the allocated memory for conversion to <see cref="RichPresenceStruct"/>
            /// </summary>
            internal void FreeMem()
            {
                for (var i = buffers.Count - 1; i >= 0; i--)
                {
                    Marshal.FreeHGlobal(buffers[i]);
                    buffers.RemoveAt(i);
                }
            }
        }
    }
}
