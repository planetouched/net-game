using Client.Entities;
using Client.Entities._Base;
using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Utils;

namespace Client.Factories
{
    public static class ClientEntityFactory
    {
        public static IClientEntity Create(NetDataReader netDataReader)
        {
            var type = (GameEntityType)netDataReader.GetByte();
            var objectId = netDataReader.PeekUInt();

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
            
            entity?.Deserialize(netDataReader);

            return entity;
        }
    }
}