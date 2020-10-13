using System.Collections.Generic;
using System.Numerics;
using Server.Entities;
using Server.Requests._Base;
using Server.Worlds._Base;
using Shared.Enums;
using Shared.Factories;
using Shared.Messages._Base;
using Shared.Messages.FromServer;
using SimpleTcp;

namespace Server.Requests
{
    public class ConnectPlayerRequest : IRequest
    {
        private readonly SimpleTcpServer _server;
        private readonly string _ipPort;
        private readonly IServerWorld _world;
        private readonly Dictionary<string, ServerPlayer> _players;
        private readonly uint _gameId;

        public ConnectPlayerRequest(
            SimpleTcpServer server,
            string ipPort,
            IServerWorld world,
            uint gameId,
            Dictionary<string, ServerPlayer> players
        )
        {
            _gameId = gameId;
            _server = server;
            _ipPort = ipPort;
            _world = world;
            _players = players;
        }

        public void Process()
        {
            var objectId = _world.GetNewObjectId();

            var player = new ServerPlayer(_ipPort)
            {
                position = new Vector3(0, 1, 0),
                rotation = Vector3.Zero
            };

            var message = MessageFactory.Create<ConnectAcceptedMessage>(MessageIds.ConnectAccepted);
            message.gameId = _gameId;
            message.objectId = objectId;

            _world.AddEntity(objectId, player);
            _players.Add(_ipPort, player);
            
            _server.Send(_ipPort, MessageBase.ConvertToBytes(message));
        }
    }
}