using System.Collections.Generic;
using Server.Entities._Base;
using Shared.Entities;
using Shared.Messages.FromClient;

namespace Server.Entities
{
    public class ServerPlayer : ServerEntityBase
    {
        private readonly SharedPlayer _sharedPlayer;

        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(256);

        public ServerPlayer(SharedPlayer sharedPlayer) : base(sharedPlayer)
        {
            _sharedPlayer = sharedPlayer;
        }
        
        public void AddControlMessage(ControlMessage message)
        {
            _controlMessages.Enqueue(message);
            _sharedPlayer.lastMessageNum  = message.messageNum;
        }

        public override void Process(float deltaTime)
        {
            while (_controlMessages.Count > 0)
            {
                var message = _controlMessages.Dequeue();
                var position = _sharedPlayer.position;
                var rotation = _sharedPlayer.rotation;
                SharedPlayerBehaviour.Movement(ref position, ref rotation, message);
                _sharedPlayer.position = position;
                _sharedPlayer.rotation = rotation;
            }
        }
    }
}