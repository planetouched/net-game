using LiteNetLib.Utils;

namespace Shared.CommonInterfaces
{
    public interface IDeserializable
    {
        void Deserialize(NetDataReader netDataReader);
    }
}