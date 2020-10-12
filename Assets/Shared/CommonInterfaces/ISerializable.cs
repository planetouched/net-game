namespace Shared.CommonInterfaces
{
    public interface ISerializable
    {
        void Serialize(ref int offset, byte[] buffer);
    }
} 