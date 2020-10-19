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

        protected MessageBase()
        {
        }

        protected MessageBase(uint messageNum, MessageIds messageId, uint objectId, int gameId)
        {
            this.messageNum = messageNum;
            this.messageId = messageId;
            this.objectId = objectId;
            this.gameId = gameId;
        }

        public abstract NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true);
        public abstract void Deserialize(NetDataReader netDataReader);

        protected void WriteHeader(NetDataWriter netDataWriter)
        {
            netDataWriter.Put((byte) messageId);
            netDataWriter.Put(messageNum);
            netDataWriter.Put(gameId);
            netDataWriter.Put(objectId);
        }

        protected void ReadHeader(NetDataReader netDataReader)
        {
            messageId = (MessageIds) netDataReader.GetByte();
            messageNum = netDataReader.GetUInt();
            gameId = netDataReader.GetInt();
            objectId = netDataReader.GetUInt();
        }
    }
}