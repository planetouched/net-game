using System.Collections.Generic;
using Shared.CommonInterfaces;
using Shared.Entities._Base;

namespace Server.Worlds._Base
{
    public interface IWorldSnapshot : ISerializable
    {
        float serverTime { get; }
        Dictionary<uint, ISharedEntity> entities { get; }
    }
}