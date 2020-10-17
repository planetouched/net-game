using LiteNetLib.Utils;
using Server.Worlds._Base;
using Shared.Entities;

namespace Server.Entities._Base
{
    public abstract class ServerEntityBase : SharedEntityBase, IServerEntity
    {
        public bool isRemoved { get; private set; }
        public IServerWorld world { get; set; }
        
        public void Remove()
        {
            isRemoved = true;
        }

        protected void WriteHeader(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter, this);
        }

        public static void WriteHeader(NetDataWriter netDataWriter, ISharedEntity entity)
        {
            netDataWriter.Put((byte)entity.type);
            netDataWriter.Put(entity.objectId);
            
            netDataWriter.Put(entity.position.X);
            netDataWriter.Put(entity.position.Y);
            netDataWriter.Put(entity.position.Z);
            
            netDataWriter.Put(entity.rotation.X);
            netDataWriter.Put(entity.rotation.Y);
            netDataWriter.Put(entity.rotation.Z);
        }

        public abstract NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true);
    }
}