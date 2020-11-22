using System;
using Client.Entities;
using Client.Entities._Base;
using Client.Worlds;
using Shared.Entities._Base;
using Shared.Enums;

namespace Client.Factories
{
    public static class ClientEntityFactory
    {
        public static ClientEntityBase Create(SharedEntityBase sharedEntity, ClientWorld clientWorld)
        {
            ClientEntityBase entity = null;
            
            switch (sharedEntity.type)
            {
                case GameEntityType.Player:
                    if (sharedEntity.objectId == ClientLocalPlayer.serverObjectId)
                    {
                        entity = new ClientLocalPlayer(clientWorld);
                    }
                    else
                    {
                        entity = new ClientPlayer(clientWorld);
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