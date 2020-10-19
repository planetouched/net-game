using LiteNetLib.Utils;
using Shared.Entities;
using Shared.Entities._Base;
using Shared.Enums;

namespace Shared.Factories
{
    public static class SharedEntityFactory
    {
        public static ISharedEntity Create(NetDataReader netDataReader)
        {
            var type = (GameEntityType)netDataReader.PeekByte();

            ISharedEntity entity = null;
            switch (type)
            {
                case GameEntityType.Player:
                    entity = new SharedPlayer();
                    break;
            }
            
            entity?.Deserialize(netDataReader);

            return entity;
        }
    }
}