namespace Shared.CommonInterfaces
{
    public interface IDeserializable
    {
        void Deserialize(ref int offset, byte[] buffer);
    }
}