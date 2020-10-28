using Client.Entities._Base;
using Shared;
using Shared.Entities;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Client.Entities.Weapons._Base
{
    public abstract class ClientLocalWeaponBase : ClientEntityBase
    {
        private bool _use;
        protected Vector3 position;
        protected Vector3 rotation;
        private float _timeToReady;
        
        public override void Process()
        {
            var weapon = (SharedWeapon) current;
            
            //only server reloads 
            if (_use && weapon.isReady && _timeToReady <= 0)
            {
                _timeToReady = SharedSettings.RailGunReloadTime;
                Shot();
            }

            if (_timeToReady > 0)
                _timeToReady -= Time.deltaTime;
        }

        public void Use(bool value)
        {
            _use = value;
        }

        public void SetPositionAndRotation(Vector3 pos, Vector3 rot)
        {
            position = pos;
            rotation = rot;
        }
        
        protected abstract void Shot();
    }
}