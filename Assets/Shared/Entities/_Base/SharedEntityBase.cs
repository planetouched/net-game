using System.Numerics;
using LiteNetLib.Utils;
using Shared.Enums;

namespace Shared.Entities._Base
{
    public abstract class SharedEntityBase : ISharedEntity
    {
        public GameEntityType type { get; protected set; }
        public uint objectId { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        
        public abstract ISharedEntity Clone();

        protected void WriteHeader(NetDataWriter netDataWriter)
        {
            netDataWriter.Put((byte)type);
            netDataWriter.Put(objectId);
            
            netDataWriter.Put(position.X);
            netDataWriter.Put(position.Y);
            netDataWriter.Put(position.Z);
            
            netDataWriter.Put(rotation.X);
            netDataWriter.Put(rotation.Y);
            netDataWriter.Put(rotation.Z);
        }

        public abstract NetDataWriter Serialize(NetDataWriter netDataWriter);       
        
        public abstract void Deserialize(NetDataReader netDataReader);

        protected void ReadHeader(NetDataReader netDataReader)
        {
            type = (GameEntityType)netDataReader.GetByte();
            objectId = netDataReader.GetUInt();
            
            var pX = netDataReader.GetFloat();
            var pY = netDataReader.GetFloat();
            var pZ = netDataReader.GetFloat();
            
            var rX = netDataReader.GetFloat();
            var rY = netDataReader.GetFloat();
            var rZ = netDataReader.GetFloat();
            
            position = new Vector3(pX, pY, pZ);
            rotation = new Vector3(rX, rY, rZ);
        }        
    }
}