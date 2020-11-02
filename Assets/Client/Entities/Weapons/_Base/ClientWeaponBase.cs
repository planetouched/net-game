using Client.Entities._Base;
using Client.Worlds;
using Shared.Entities;
using Vector3 = System.Numerics.Vector3;

namespace Client.Entities.Weapons._Base
{
    public abstract class ClientWeaponBase : ClientEntityBase
    {
        protected Vector3 position;
        protected Vector3 rotation;

        private SharedWeapon _lastWeapon;
        
        protected ClientWeaponBase(ClientWorld clientWorld) : base(clientWorld)
        {
        }
        
        public override void Process()
        {
            var weapon = (SharedWeapon) current;
            
            if (weapon.shot && _lastWeapon != weapon)
            {
                _lastWeapon = weapon;
                Shot(weapon.hitTo);
            }
        }
        
        public void SetPositionAndRotation(Vector3 pos, Vector3 rot)
        {
            position = pos;
            rotation = rot;
        }

        protected abstract void Shot(uint targetId);
    }
}