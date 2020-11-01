using Server.Worlds._Base;
using Shared.Entities._Base;

namespace Server.Entities._Base
{
    public interface IServerEntity
    {
        ISharedEntity sharedEntity { get; }
        bool isRemoved { get; }
        IServerWorld world { get; set; }
        void Remove();
        void Process(float deltaTime);
       
    }
}