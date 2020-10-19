using System;
using Client.Entities;
using Client.Entities._Base;
using Shared.Entities._Base;
using Shared.Enums;

namespace Client.Factories
{
    public static class ClientEntityFactory
    {
        public static IClientEntity Create(ISharedEntity sharedEntity)
        {
            IClientEntity entity = null;
            
            switch (sharedEntity.type)
            {
                case GameEntityType.Player:
                    if (sharedEntity.objectId == ClientLocalPlayer.localObjectId)
                    {
                        entity = new ClientLocalPlayer();
                    }
                    else
                    {
                        entity = new ClientPlayer();
                    }
                    
                    break;
            }

            if (entity == null)
            {
                throw new Exception("Entity not found");
            }
            
            entity.SetCurrentEntity(sharedEntity);

            return entity;
        }
    }
}