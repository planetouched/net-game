using LiteNetLib.Utils;

namespace Shared.CommonInterfaces
{
    public interface ISerializable
    {
        NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true);
    }
} 