using LiteNetLib.Utils;
using Shared.Entities._Base;

namespace Shared.Entities
{
    public class SharedPlayer : SharedEntityBase
    {
        public uint lastMessageNum { get; set; }
        
        public override NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true)
        {
            if (resetBeforeWriting)
            {
                netDataWriter.Reset();
            }

            WriteHeader(netDataWriter);
            netDataWriter.Put(lastMessageNum);
            
            return netDataWriter;        
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            lastMessageNum = netDataReader.GetUInt();        
        }
    }
}