using Shared.Enums;
using Shared.Factories;
using Shared.Utils;

namespace Shared.Messages._Base
{
    public abstract class MessageBase : IMessage
    {
        public int messageSize { get; private set; }
        public uint gameId { get; set; }
        public byte messageId { get; protected set; }
        public uint objectId { get; set; }

        protected const int HeaderSize = 13;

        public abstract void Serialize(ref int offset, byte[] buffer);
        public abstract void Deserialize(ref int offset, byte[] buffer);

        public MessageIds GetMessageId()
        {
            return (MessageIds) messageId;
        }

        public abstract int MessageSize();

        protected void WriteHeader(ref int offset, byte[] buffer)
        {
            offset += 4;
            SerializeUtil.SetUInt(gameId, ref offset, buffer);
            SerializeUtil.SetByte(messageId, ref offset, buffer);
            SerializeUtil.SetUInt(objectId, ref offset, buffer);
        }

        protected void SetMessageSize(int mSize, byte[] buffer)
        {
            int offset = 0;
            messageSize = mSize;
            SerializeUtil.SetInt(mSize, ref offset, buffer);
        }

        protected void ReadHeader(ref int offset, byte[] buffer)
        {
            messageSize = SerializeUtil.GetInt(ref offset, buffer);
            gameId = SerializeUtil.GetUInt(ref offset, buffer);
            messageId = SerializeUtil.GetByte(ref offset, buffer);
            objectId = SerializeUtil.GetUInt(ref offset, buffer);
        }

        public static MessageIds GetMessageId(byte[] buffer)
        {
            int offset = 0;
            SerializeUtil.GetUInt(ref offset, buffer);
            var messageId = (MessageIds) SerializeUtil.GetByte(ref offset, buffer);
            return messageId;
        }

        public static byte[] ConvertToBytes(IMessage message)
        {
            var outputBuffer = new byte[message.MessageSize()];
            int messageOffset = 0;
            message.Serialize(ref messageOffset, outputBuffer);
            return outputBuffer;
        }
    }
}