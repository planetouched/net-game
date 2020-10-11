﻿using Shared.Game.Entities;
using Shared.Game.Entities._Base;

namespace Server.Game.Entities._Base
{
    public interface IServerWorld : ISerializable
    {
        uint AddEntity(uint objectId, IServerEntity entity);
        void RemoveEntity(uint objectId);
        IServerEntity FindEntity(uint objectId);
        T FindEntity<T>(uint objectId, GameEntityType type) where T : class;

        void Process();
        uint GetNewObjectId();
    }
}