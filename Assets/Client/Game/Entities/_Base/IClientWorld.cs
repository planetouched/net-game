using Shared.Game.Entities;

namespace Client.Game.Entities._Base
{
    public interface IClientWorld
    {
        IClientEntity FindEntity(uint objectId);
        T FindEntity<T>(uint objectId, GameEntityType type) where T : class;
        void Deserialize(ref int offset, byte[] buffer, int bufferSize);
    }
}