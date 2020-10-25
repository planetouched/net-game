using LiteNetLib.Utils;
using Shared.Enums;

namespace Shared.Messages._Base
{
    public abstract class MessageBase : IMessage
    {
        public MessageIds messageId { get; private set; }
        public uint messageNum { get; private set; }
        public int gameId { get; private set; }
        public uint objectId { get; private set; }
        //public float time { get; private set;}
        
        // public IMessage SetTime(float t)
        // {
        //     time = t;
        //     return this;
        // }

        public IMessage SetGameId(int id)
        {
            gameId = id;
            return this;
        }

        public IMessage SetObjectId(uint id)
        {
            objectId = id;
            return this;
        }

        public IMessage SetMessageNum(uint num)
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
            netDataWriter.Put(messageNum);
            netDataWriter.Put(gameId);
            netDataWriter.Put(objectId);
            //netDataWriter.Put(time);
        }

        protected void ReadHeader(NetDataReader netDataReader)
        {
            messageId = (MessageIds) netDataReader.GetByte();
            messageNum = netDataReader.GetUInt();
            gameId = netDataReader.GetInt();
            objectId = netDataReader.GetUInt();
            //time = netDataReader.GetFloat();
        }
    }
}