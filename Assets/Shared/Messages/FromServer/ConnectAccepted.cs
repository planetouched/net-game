using System;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromServer
{
    public class ConnectAccepted : MessageBase
    {
        public ConnectAccepted()
        {
            messageId = (byte)MessageIds.ConnectAccepted;
        }
        
        public override void Serialize(ref int offset, byte[] buffer)
        {
            WriteHeader(ref offset, buffer);
            SetMessageSize(offset, buffer);            
        }

        public override void Deserialize(ref int offset, byte[] buffer)
        {
            ReadHeader(ref offset, buffer);
            
            if (MessageSize() != messageSize)
            {
                throw new Exception("MessageSize() != messageSize");
            }
        }
        
        public override int MessageSize()
        {
            return HeaderSize;
        }
    }
}