using Shared.CommonInterfaces;
using Shared.Game.Entities._Base;
using Shared.Messages._Base;

namespace Client.Game.Entities._Base
{
    public interface IClientEntity : ISharedEntity, IDeserializable
    {
    }
}