using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromClient
{
    public class EnterGameMessage : MessageBase
    {
        public EnterGameMessage() : base(MessageIds.EnterGame)
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