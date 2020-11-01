using Basement.OEPFramework.UnityEngine._Base;
using Shared.Entities._Base;
using Shared.Enums;

namespace Client.Entities._Base
{
    public interface IClientEntity : IDroppableItem
    {
        GameEntityType type { get; }
        uint objectId { get; }
        bool isUsed { get; }
        void Create();
        void UnUse();
        void Use();
        void SetCurrentEntity(ISharedEntity entity);
        void SetSnapshotDeltaTime(float sDeltaTime);
        void Process();
    }
}