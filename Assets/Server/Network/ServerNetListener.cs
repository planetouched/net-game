using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;
using Shared.Factories;
using Shared.Loggers;
using Shared.Messages._Base;

namespace Server.Network
{
    public class ServerNetListener : INetEventListener
    {
        public NetManager netManager { get; }

        private readonly int _port;

        public event Action<NetPeer, IMessage> onIncomingMessage;
        public event Action<NetPeer> onClientDisconnected;
        public event Action<NetPeer> onClientConnected;

        public bool isStarted { get; private set; }

        public ServerNetListener(int port)
        {
            if (port != -1)
            {
                netManager = new NetManager(this, new Crc32cLayer()) {UpdateTime = 15};
            }

            _port = port;
        }

        public void Start()
        {
            //var ip4 = IPAddress.Any.ToString();
            //var ip6 = IPAddress.IPv6Any.ToString();
            isStarted = netManager.Start(_port);
        }

        public void Stop()
        {
            isStarted = false;
            netManager.Stop();
        }

        public void PollEvents()
        {
            netManager.PollEvents();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Logger.Log("[SERVER] We have new peer " + peer.EndPoint);
            onClientConnected?.Invoke(peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Logger.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
            onClientDisconnected?.Invoke(peer);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Logger.Log("[SERVER] error " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            while (reader.AvailableBytes > 0)
            {
                var message = MessageFactory.Create(reader);
                onIncomingMessage?.Invoke(peer, message);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Logger.Log("[SERVER] OnNetworkReceiveUnconnected");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Logger.Log("[SERVER] OnNetworkLatencyUpdate");
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey("net-game");
        }
    }
}