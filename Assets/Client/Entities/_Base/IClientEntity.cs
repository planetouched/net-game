using Basement.OEPFramework.UnityEngine._Base;
using Shared.CommonInterfaces;
using Shared.Entities;

namespace Client.Entities._Base
{
    public interface IClientEntity : ISharedEntity, IDeserializable, IDroppableItem
    {
        bool isUsed { get; }
        void Create();
        void UnUse();
        void Use();
    }
}