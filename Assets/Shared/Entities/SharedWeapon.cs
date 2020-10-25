using LiteNetLib.Utils;
using Shared.Entities._Base;
using Shared.Enums;

namespace Shared.Entities
{
    public class SharedWeapon : SharedEntityBase
    {
        public bool isInstant { get; } = true;
        
        public bool isReady => timeToReady <= 0;
        public float timeToReady { get; set; }
        public bool shot { get; set; }

        public SharedWeapon()
        {
            type = GameEntityType.Weapon;
        }
        
        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);
            netDataWriter.Put(timeToReady);
            netDataWriter.Put(shot);
            
            return netDataWriter;        
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            timeToReady = netDataReader.GetFloat();
            shot = netDataReader.GetBool();
        }
    }
}