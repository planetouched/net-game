namespace Server.Game.Entities._Base
{
    public interface ISerializable
    {
        void Serialize(ref int offset, byte[] buffer);
    }
}