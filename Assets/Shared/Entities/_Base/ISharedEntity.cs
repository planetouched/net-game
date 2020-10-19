using System.Numerics;
using Shared.CommonInterfaces;
using Shared.Enums;

namespace Shared.Entities._Base
{
    public interface ISharedEntity : ISerializable, IDeserializable
    {
        GameEntityType type { get; }
        uint objectId { get; set; }
        Vector3 position { get; set; }
        Vector3 rotation { get; set; }
    }
}