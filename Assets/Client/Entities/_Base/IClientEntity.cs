using Shared.CommonInterfaces;
using Shared.Entities;

namespace Client.Entities._Base
{
    public interface IClientEntity : ISharedEntity, IDeserializable
    {
        void Drop();
    }
}