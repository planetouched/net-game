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
            int localOffset = offset;
            var type = (GameEntityType)SerializeUtil.GetByte(ref localOffset, buffer);
            var objectId = SerializeUtil.GetUInt(ref localOffset, buffer);

            IClientEntity entity = null;
            switch (type)
            {
                case GameEntityType.Player:
                    if (objectId == ClientLocalPlayer.localObjectId)
                    {
                        entity = new ClientLocalPlayer();
                    }
                    else
                    {
                        entity = new ClientPlayer();
                    }
                    break;
            }
            
            entity?.Deserialize(ref offset, buffer);

            return entity;
        }
    }
}