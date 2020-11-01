using LiteNetLib.Utils;
using Shared.Entities._Base;
using Shared.Enums;

namespace Shared.Entities
{
    public class SharedPlayer : SharedEntityBase
    {
        public uint lastMessageNum { get; set; }
        public bool isAlive { get; set; }
        
        public SharedWeapon weapon { get; set; }

        public SharedPlayer()
        {
            type = GameEntityType.Player;
        }

        public override ISharedEntity Clone()
        {
            var clone = new SharedPlayer
            {
                isAlive = isAlive,
                position = position,
                rotation = rotation,
                objectId = objectId,
                lastMessageNum = lastMessageNum,
                weapon = (SharedWeapon) weapon.Clone()
            };
            
            return clone;
        }

        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);
            netDataWriter.Put(lastMessageNum);
            netDataWriter.Put(isAlive);
            weapon.Serialize(netDataWriter);
            
            return netDataWriter;        
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            lastMessageNum = netDataReader.GetUInt();
            isAlive = netDataReader.GetBool();
            weapon = new SharedWeapon();
            weapon.Deserialize(netDataReader);
        }
    }
}