using System.Numerics;
using Server.Game.Entities._Base;
using Shared.Enums;

namespace Shared.Game.Entities._Base
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