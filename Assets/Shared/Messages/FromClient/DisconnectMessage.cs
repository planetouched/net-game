using Shared.Enums;
using Shared.Factories;
using Shared.Messages._Base;

namespace Shared.Messages.FromClient
{
    public class DisconnectMessage : MessageBase
    {
        public DisconnectMessage()
        {
            messageId = (byte)MessageIds.Disconnect;
        }
        
        public override void Serialize(ref int offset, byte[] buffer)
        {
        }

        public override void Deserialize(ref int offset, byte[] buffer)
        {
        }
        
        public override int MessageSize()
        {
            return HeaderSize;
        }
    }
}