using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromServer
{
    public class EnterGameAcceptedMessage : MessageBase
    {
        public EnterGameAcceptedMessage()
        {
        }
        
        public EnterGameAcceptedMessage(uint messageNum, uint objectId, int gameId) : base(messageNum, MessageIds.ConnectAccepted, objectId, gameId)
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