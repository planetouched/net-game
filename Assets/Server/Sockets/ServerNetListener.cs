using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using Shared.Factories;
using Shared.Messages._Base;

namespace Server.Sockets
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
            netManager = new NetManager(this);
            netManager.BroadcastReceiveEnabled = true;
            netManager.UpdateTime = 15;
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
        
        public void OnPeerConnected(NetPeer peer)
        {
            UnityEngine.Debug.Log("[SERVER] We have new peer " + peer.EndPoint);
            onClientConnected?.Invoke(peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            UnityEngine.Debug.Log("[SERVER] peer disconnected " + peer.EndPoint + ", info: " + disconnectInfo.Reason);            
            onClientDisconnected?.Invoke(peer);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            UnityEngine.Debug.Log("[SERVER] error " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var message = MessageFactory.Create(reader);
            onIncomingMessage?.Invoke(peer, message);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            UnityEngine.Debug.Log("[SERVER] OnNetworkReceiveUnconnected");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //UnityEngine.Debug.Log("[SERVER] OnNetworkLatencyUpdate");
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey("net-game");
        }
    }
}