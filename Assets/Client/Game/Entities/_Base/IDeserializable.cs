namespace Client.Game.Entities._Base
{
    public interface IDeserializable
    {
        void Deserialize(ref int offset, byte[] buffer);
    }
}