using System.Numerics;
using Server.Entities._Base;
using Shared;
using Shared.Entities;

namespace Server.Entities.Weapons
{
    public class ServerWeapon : ServerEntityBase
    {
        private readonly SharedWeapon _sharedWeapon;

        public bool shot => _sharedWeapon.shot;

        public bool isInstant => _sharedWeapon.isInstant;

        public ServerWeapon(SharedWeapon sharedWeapon) : base(sharedWeapon)
        {
            _sharedWeapon = sharedWeapon;
        }

        public void Prepare()
        {
            _sharedWeapon.shot = false;
        }
        
        public void Use(bool value)
        {
            if (value && _sharedWeapon.isReady)
            {
                _sharedWeapon.timeToReady = SharedSettings.RailGunReloadTime;
                _sharedWeapon.shot = true;
            }
        }

        public void SetPositionAndRotation(Vector3 position, Vector3 rotation)
        {
            sharedEntity.position = position;
            sharedEntity.rotation = rotation;
        }
        
        public override void Process(float deltaTime)
        {
            if (_sharedWeapon.timeToReady > 0)
            {
                _sharedWeapon.timeToReady -= deltaTime;
            }
        }
    }
}