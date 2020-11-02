using System.Collections.Generic;
using Server.Entities._Base;
using Server.Entities.Weapons;
using Shared.Entities;
using Shared.Messages.FromClient;

namespace Server.Entities
{
    public class ServerPlayer : ServerEntityBase
    {
        private readonly SharedPlayer _sharedPlayer;
        public ServerWeapon weapon { get; }

        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(256);

        public ServerPlayer(SharedPlayer sharedPlayer) : base(sharedPlayer)
        {
            _sharedPlayer = sharedPlayer;
            _sharedPlayer.isAlive = true;
            _sharedPlayer.weapon = new SharedWeapon();
            
            weapon = new ServerWeapon(_sharedPlayer.weapon);
        }
        
        public void AddControlMessage(ControlMessage message)
        {
            _controlMessages.Enqueue(message);
            _sharedPlayer.lastMessageNum  = message.messageNum;
        }

        public override void Process(float deltaTime)
        {
            weapon.Prepare();
            
            while (_controlMessages.Count > 0)
            {
                var message = _controlMessages.Dequeue();
                var position = _sharedPlayer.position;
                var rotation = _sharedPlayer.rotation;
                SharedPlayerBehaviour.Movement(ref position, ref rotation, message);
                _sharedPlayer.position = position;
                _sharedPlayer.rotation = rotation;

                weapon.Use(message.mouseButton0);

                if (weapon.shot && !weapon.isLocked)
                {
                    var target = world.Shot(this, message.serverTime);
                    weapon.Lock();
                    weapon.HitTo(target);
                }
            }
            
            weapon.Process(deltaTime);
        }
    }
}