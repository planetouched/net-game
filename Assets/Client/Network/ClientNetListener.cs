using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Layers;
using Shared.Factories;
using Shared.Loggers;
using Shared.Messages._Base;

namespace Client.Network
{
    public class ClientNetListener : INetEventListener
    {
        //network
        private readonly string _serverIp;
        private readonly int _port;

        public NetManager netManager { get; }
        public NetPeer netPeer { get; private set; }
        
        public event Action<IMessage> onIncomingMessage;
        public event Action onConnect;
        public event Action onDisconnect;

        public bool IsConnected => netPeer != null && netPeer.ConnectionState == ConnectionState.Connected;
        
        public bool isStarted { get; private set; }
    
        public ClientNetListener(string serverIp, int port)
        {
            _serverIp = serverIp;
            _port = port;

            if (port != -1)
            {
                netManager = new NetManager(this, new Crc32cLayer());
                netManager.UpdateTime = 5;
                netManager.Start();
            }
        }

        public void Start()
        {
            netPeer = netManager.Connect(_serverIp, _port, "net-game");
        }

        public void Stop()
        {
            netPeer.Disconnect();
            netPeer = null;
        }

        public void PollEvents()
        {
            netManager.PollEvents();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Log.Write("[CLIENT] We connected to " + peer.EndPoint);
            isStarted = true;
            onConnect?.Invoke();
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Write("[CLIENT] We disconnected because " + disconnectInfo.Reason);
            isStarted = false;
            onDisconnect?.Invoke();
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Log.Write("[CLIENT] We received error " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var message = MessageFactory.Create(reader);
            //Logger.Log("[CLIENT] We received message " + message.messageId);
            onIncomingMessage?.Invoke(message);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Log.Write("[CLIENT] OnNetworkReceiveUnconnected");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Logger.Log("[CLIENT] OnNetworkLatencyUpdate");
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Log.Write("[CLIENT] OnConnectionRequest");
        }
    }
}