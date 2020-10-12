using Client.Game.Entities;
using Client.Game.Entities._Base;
using Shared.Enums;
using Shared.Game.Entities;
using Shared.Utils;

namespace Client.Game.Factories
{
    public static class ClientEntityFactory
    {
        public static IClientEntity Create(ref int offset, byte[] buffer)
        {
            var type = (GameEntityType)SerializeUtil.GetInt(ref offset, buffer, false);

            IClientEntity entity = null;
            switch (type)
            {
                case GameEntityType.Player:
                    entity = new ClientPlayer();
                    break;
            }
            
            entity?.Deserialize(ref offset, buffer);

            return entity;
        }
    }
}