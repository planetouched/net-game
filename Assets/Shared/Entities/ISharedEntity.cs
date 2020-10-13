using System.Numerics;
using Shared.Enums;

namespace Shared.Entities
{
    public interface ISharedEntity
    {
        GameEntityType type { get; }
        uint objectId { get; set; }
        Vector3 position { get; set; }
        Vector3 rotation { get; set; }
        
        void Process();
    }
}