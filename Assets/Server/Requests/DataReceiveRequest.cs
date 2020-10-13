using System.Collections.Generic;
using Server.Entities;
using Server.Requests._Base;
using Shared.Factories;
using Shared.Messages._Base;

namespace Server.Requests
{
    public class DataReceiveRequest : IRequest
    {
        private readonly string _ipPort;
        private readonly byte[] _bytes;
        private readonly Dictionary<string, ServerPlayer> _players;
        private readonly Queue<IMessage> _messages;

        public DataReceiveRequest(string ipPort, byte[] bytes, Dictionary<string, ServerPlayer> players, Queue<IMessage> messages)
        {
            _messages = messages;
            _ipPort = ipPort;
            _bytes = bytes;
            _players = players;
        }
        
        public void Process()
        {
            if (_players.TryGetValue(_ipPort, out var player))
            {
                var fullMessages = player.byteToMessageDecoder.Decode(_bytes);
                
                if (fullMessages != null)
                {
                    for (int i = 0; i < fullMessages.Count; i++)
                    {
                        _messages.Enqueue(MessageFactory.Create(fullMessages[i]));
                    }
                }
            }
        }
    }
}