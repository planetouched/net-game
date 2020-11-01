using Server.Entities._Base;
using Shared.Enums;
using Shared.Messages.FromClient;

namespace Server.Worlds._Base
{
    public interface IServerWorld
    {
        float time { get; }
        uint AddEntity(uint objectId, IServerEntity entity);
        void RemoveEntity(uint objectId);
        IServerEntity FindEntity(uint objectId);
        T FindEntity<T>(uint objectId, GameEntityType type) where T : class;
        bool Exists(uint objectId);
        void Shot(IServerEntity shooter, ControlMessage message);

        void Process();
        uint GetNewObjectId();
        void SetupTime();
        IWorldSnapshot CreateSnapshot(float serverTime, bool keep);
    }
}