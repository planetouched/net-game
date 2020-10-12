using Basement.OEPFramework.UnityEngine._Base;
using Shared.Game.Entities._Base;
using UnityEngine;

namespace Client.Game.Views
{
    public class WorldView : DroppableItemBase
    {
        private readonly ISharedPlayer _localPlayer;
        
        public WorldView(ISharedPlayer localPlayer)
        {
            _localPlayer = localPlayer;
        }

        public void AddWorldSnapshot(ClientWorldSnapshot clientWorldSnapshot)
        {
        }

        public void Update()
        {
            var r = _localPlayer.rotation;
            var p = _localPlayer.position;
            var rotation = Quaternion.Euler(new Vector3(r.X, r.Y, r.Z));
            var position = new Vector3(p.X, p.Y, p.Z);
            
            Camera.main.transform.rotation = rotation;
            Camera.main.transform.position = position;
        }
    }
}