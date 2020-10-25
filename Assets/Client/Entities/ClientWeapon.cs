using Client.Entities._Base;
using Client.Utils;
using Shared.Entities;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Client.Entities
{
    public class ClientWeapon : ClientEntityBase
    {
        private bool _lockShot;
        private Vector3 _position;
        private Vector3 _rotation;
        
        public override void Process()
        {
            var weapon = (SharedWeapon) current;
            
            if (!weapon.shot)
            {
                _lockShot = false;
            }
            
            if (weapon.shot && !_lockShot)
            {
                _lockShot = true;

                Shot();
            }
        }

        public void SetPositionAndRotation(Vector3 position, Vector3 rotation)
        {
            _position = position;
            _rotation = rotation;
        }

        private void Shot()
        {
            var obj = Object.Instantiate(Resources.Load<GameObject>("RailGun/Prefabs/RailGun"));
            obj.transform.position = _position.ToUnity();
            obj.transform.rotation = Quaternion.Euler(_rotation.ToUnity());
            obj.transform.Translate(UnityEngine.Vector3.forward * 0.5f);
        }
        
        public override void Create()
        {
        }
    }
}