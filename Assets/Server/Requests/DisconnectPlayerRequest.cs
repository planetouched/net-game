using System.Collections.Generic;
using Server.Entities;
using Server.Requests._Base;
using Server.Worlds._Base;

namespace Server.Requests
{
    public class DisconnectPlayerRequest : IRequest
    {
        private readonly string _ipPort;
        private readonly IServerWorld _world;
        private readonly Dictionary<string, ServerPlayer> _players;
        
        public DisconnectPlayerRequest(string ipPort, IServerWorld world, Dictionary<string, ServerPlayer> players)
        {
            _ipPort = ipPort;
            _world = world;
            _players = players;
        }
        
        public void Process()
        {
            if (_players.ContainsKey(_ipPort))
            {
                var objectId = _players[_ipPort].objectId;
                _world.RemoveEntity(objectId);
                _players.Remove(_ipPort);
            }
        }
    }
}