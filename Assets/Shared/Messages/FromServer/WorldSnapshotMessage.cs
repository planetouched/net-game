using System;
using Shared.Enums;
using Shared.Messages._Base;
using Shared.Utils;

namespace Shared.Messages.FromServer
{
    public class WorldSnapshotMessage : MessageBase
    {
        public uint snapshotNum { get; set; }
        public int snapshotSize { get; set; }
        public byte[] data { get; set; }

        public WorldSnapshotMessage()
        {
            messageId = (byte) MessageIds.WorldSnapshot;
        }

        public override void Serialize(ref int offset, byte[] buffer)
        {
            WriteHeader(ref offset, buffer);
            SerializeUtil.SetUInt(snapshotNum, ref offset, buffer);
            SerializeUtil.SetInt(snapshotSize, ref offset, buffer);
            SerializeUtil.BlockCopy(data, 0, buffer, offset, snapshotSize, ref offset);
            SetMessageSize(offset, buffer);            
        }

        public override void Deserialize(ref int offset, byte[] buffer)
        {
            ReadHeader(ref offset, buffer);
            snapshotNum = SerializeUtil.GetUInt(ref offset, buffer);
            snapshotSize = SerializeUtil.GetInt(ref offset, buffer);
            
            if (MessageSize() != messageSize)
            {
                throw new Exception("MessageSize() != messageSize");
            }
            
            SerializeUtil.BlockCopy(buffer, offset, data, 0, snapshotSize, ref offset);
        }
        
        public override int MessageSize()
        {
            return HeaderSize + snapshotSize + 8;
        }
    }
}