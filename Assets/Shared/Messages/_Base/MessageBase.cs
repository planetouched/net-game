using LiteNetLib.Utils;
using Shared.CommonInterfaces;
using Shared.Enums;

namespace Shared.Messages._Base
{
    public abstract class MessageBase : ISerializable, IDeserializable
    {
        public MessageIds messageId { get; private set; }
        public uint messageNum { get; private set; }
        public int worldId { get; private set; }
        public uint objectId { get; private set; }
        public bool system { get; protected set; }

        public MessageBase SetWorldId(int id)
        {
            worldId = id;
            return this;
        }

        public MessageBase SetObjectId(uint id)
        {
            objectId = id;
            return this;
        }

        public MessageBase SetMessageNum(uint num)
        {
            messageNum = num;
            return this;
        }

        protected MessageBase()
        {
        }

        protected MessageBase(MessageIds messageId)
        {
            this.messageId = messageId;
        }

        public abstract NetDataWriter Serialize(NetDataWriter netDataWriter);
        public abstract void Deserialize(NetDataReader netDataReader);

        protected void WriteHeader(NetDataWriter netDataWriter)
        {
            netDataWriter.Put((byte) messageId);
            netDataWriter.Put(system);
            netDataWriter.Put(messageNum);
            netDataWriter.Put(worldId);
            netDataWriter.Put(objectId);
        }

        protected void ReadHeader(NetDataReader netDataReader)
        {
            messageId = (MessageIds) netDataReader.GetByte();
            system = netDataReader.GetBool();
            messageNum = netDataReader.GetUInt();
            worldId = netDataReader.GetInt();
            objectId = netDataReader.GetUInt();
        }
    }
}