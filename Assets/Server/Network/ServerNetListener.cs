using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Layers;
using Shared.Factories;
using Shared.Loggers;
using Shared.Messages._Base;

namespace Server.Network
{
    public class ServerNetListener : INetEventListener
    {
        public NetManager netManager { get; }

        private readonly int _port;

        public event Action<NetPeer, MessageBase> onIncomingMessage;
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
            Log.Write("[SERVER] We have new peer " + peer.EndPoint);
            Log.Write("Peer -> MTU: " + peer.Mtu);
            onClientConnected?.Invoke(peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Write("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);
            onClientDisconnected?.Invoke(peer);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Log.Write("[SERVER] error " + socketError);
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
            Log.Write("[SERVER] OnNetworkReceiveUnconnected");
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