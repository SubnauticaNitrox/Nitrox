using System.Threading.Tasks;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    /// <summary>
    /// Abstracted IClient in order to give us options in the underlying protocol that we use to communicate with the server.
    /// Ex: We may want to also roll a UDP client in the future to handle packets where we don't necessarily care
    /// about transmission order or error recovery.
    /// </summary>
    public interface IClient
    {
        bool IsConnected { get; }
        Task StartAsync(string ipAddress, int serverPort);
        void Stop();
        void Send(Packet packet);
    }
}
