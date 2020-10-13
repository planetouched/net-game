using System.Collections.Generic;
using Server.Entities;
using Server.Requests._Base;

namespace Server.Requests
{
    public class DisconnectPlayerRequest : IRequest
    {
        private readonly string _ipPort;
        private readonly Dictionary<string, ServerPlayer> _players;
        
        public DisconnectPlayerRequest(string ipPort, Dictionary<string, ServerPlayer> players)
        {
            _ipPort = ipPort;
            _players = players;
        }
        
        public void Process()
        {
            if (_players.ContainsKey(_ipPort))
            {
                var entity = _players[_ipPort];
                entity.Remove();
                _players.Remove(_ipPort);
            }
        }
    }
}