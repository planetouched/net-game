using Basement.OEPFramework.UnityEngine._Base;
using Shared.Game;
using UnityEngine;

namespace Client.Game
{
    public class WorldView : DroppableItemBase
    {
        private readonly World _world;
        
        public WorldView(World world)
        {
            _world = world;
        }

        public void Update()
        {
            var r = _world.localPlayer.rotation;
            var p = _world.localPlayer.position;
            var rotation = Quaternion.Euler(new Vector3(r.X, r.Y, r.Z));
            var position = new Vector3(p.X, p.Y, p.Z);
            
            Camera.main.transform.rotation = rotation;
            Camera.main.transform.position = position;
        }
    }
}