using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromServer
{
    public class EnterGameAcceptedMessage : MessageBase
    {
        public EnterGameAcceptedMessage() : base(MessageIds.ConnectAccepted)
        {
        }
        
        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);
            
            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
        }
    }
}