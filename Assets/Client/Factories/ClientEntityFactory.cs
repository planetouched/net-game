using Client.Entities;
using Client.Entities._Base;
using Shared.Enums;
using Shared.Utils;

namespace Client.Factories
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