using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromClient
{
    public class EnterGameMessage : MessageBase
    {
        public EnterGameMessage()
        {
        }
        
        public EnterGameMessage(uint messageNum) : base(messageNum, MessageIds.EnterGame, 0, -1)
        {
        }
        
        public override NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true)
        {
            if (resetBeforeWriting)
                netDataWriter.Reset();
                
            WriteHeader(netDataWriter);
            
            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
        }
    }
}