using System.Collections.Generic;
using Basement.OEPFramework.UnityEngine._Base;
using Client.Entities;
using Client.Entities._Base;

namespace Client.Worlds
{
    public class ClientWorld : DroppableItemBase
    {
        private readonly Dictionary<uint, IClientEntity> _entities = new Dictionary<uint, IClientEntity>();
        private readonly List<ClientWorldSnapshot> _snapshots = new List<ClientWorldSnapshot>();
        
        public ClientWorld(ClientPlayer localPlayer)
        {
            _entities.Add(localPlayer.objectId, localPlayer);
        }

        public void AddWorldSnapshot(ClientWorldSnapshot snapshot)
        {
            _snapshots.Add(snapshot);
            
            if (_snapshots.Count > 256)
            {
                _snapshots.RemoveAt(0);
            }
        }

        public void Update()
        {
            foreach (var entity in _entities)
            {
                entity.Value.Process();;
            }
        }
    }
}