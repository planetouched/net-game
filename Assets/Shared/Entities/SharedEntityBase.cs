using System.Numerics;
using Shared.Enums;

namespace Shared.Entities
{
    public abstract class SharedEntityBase : ISharedEntity
    {
        public GameEntityType type { get; protected set; }
        public uint objectId { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        
        public abstract void Process();
    }
}