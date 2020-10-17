using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Layers;
using Shared.Factories;
using Shared.Messages._Base;
using UnityEngine;

namespace Client.Network
{
    public class ClientNetListener : INetEventListener
    {
        //network
        private readonly string _serverIp;
        private readonly int _port;

        public NetManager netManager { get; }
        public NetPeer clientPeer { get; private set; }
        
        public event Action<IMessage> onIncomingMessage;
        public event Action onConnect;
        public event Action onDisconnect;

        public bool IsConnected => clientPeer != null && clientPeer.ConnectionState == ConnectionState.Connected;
    
        public ClientNetListener(string serverIp, int port)
        {
            _serverIp = serverIp;
            _port = port;
            
            netManager = new NetManager(this, new Crc32cLayer());
            netManager.UpdateTime = 5;
            netManager.Start();
        }

        public void Start()
        {
            clientPeer = netManager.Connect(_serverIp, _port, "net-game");
        }

        public void Stop()
        {
            clientPeer.Disconnect();
            clientPeer = null;
        }
        
        public void OnPeerConnected(NetPeer peer)
        {
            Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
            onConnect?.Invoke();
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
            onDisconnect?.Invoke();
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Debug.Log("[CLIENT] We received error " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var message = MessageFactory.Create(reader);
            //Debug.Log("[CLIENT] We received message " + message.messageId);
            onIncomingMessage?.Invoke(message);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Debug.Log("[CLIENT] OnNetworkReceiveUnconnected");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Debug.Log("[CLIENT] OnNetworkLatencyUpdate");
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Debug.Log("[CLIENT] OnConnectionRequest");
        }
    }
}