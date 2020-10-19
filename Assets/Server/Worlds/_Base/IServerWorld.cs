using Server.Entities._Base;
using Shared.CommonInterfaces;
using Shared.Enums;

namespace Server.Worlds._Base
{
    public interface IServerWorld : ISerializable
    {
        uint AddEntity(uint objectId, IServerEntity entity);
        void RemoveEntity(uint objectId);
        IServerEntity FindEntity(uint objectId);
        T FindEntity<T>(uint objectId, GameEntityType type) where T : class;
        bool Exists(uint objectId);

        void Process(float deltaTime);
        uint GetNewObjectId();
    }
}