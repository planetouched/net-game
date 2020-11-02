using Client.Entities._Base;
using Client.Entities.Weapons;
using Client.Entities.Weapons._Base;
using Client.Utils;
using Client.Worlds;
using Shared.Entities;
using Shared.Entities._Base;
using UnityEngine;

namespace Client.Entities
{
    public class ClientPlayer : ClientEntityBase
    {
        private GameObject _go;
        private float _progress;
        
        public ClientWeaponBase weapon { get; }

        public ClientPlayer(ClientWorld clientWorld) : base(clientWorld)
        {
            weapon = new ClientRailGun(clientWorld);
        }
        
        public override void SetCurrentEntity(ISharedEntity entity)
        {
            _progress = 0;
            var sharedPlayer = (SharedPlayer) entity;
            
            weapon.SetCurrentEntity(sharedPlayer.weapon);
            base.SetCurrentEntity(sharedPlayer);
        }

        public override void Create()
        {
            _go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _go.transform.rotation = Quaternion.Euler(current.rotation.ToUnity());
            _go.transform.position = current.position.ToUnity();
        }

        public override void Process()
        {
            if (previous != null)
            {
                _progress += Time.smoothDeltaTime;
                var k = _progress / serverDeltaTime;
                var prevPosition = previous.position.ToUnity();
                var currentPosition = current.position.ToUnity();
                var lerpPosition = Vector3.Lerp(prevPosition, currentPosition, k);
                //_go.transform.position = lerpPosition; 
                
                //it looks better, but what is about accuracy?
                var speed = Vector3.Distance(prevPosition, currentPosition) / serverDeltaTime;
                _go.transform.position = Vector3.MoveTowards(_go.transform.position, lerpPosition, speed * Time.smoothDeltaTime); 
                
                _go.transform.rotation = Quaternion.Lerp(Quaternion.Euler(previous.rotation.ToUnity()), Quaternion.Euler(current.rotation.ToUnity()), k);
            }
            else
            {
                _go.transform.rotation = Quaternion.Euler(current.rotation.ToUnity());
                _go.transform.position = current.position.ToUnity();
            }
            
            weapon.SetPositionAndRotation(current.position, current.rotation);
            weapon.Process();
        }

        public override void Drop()
        {
            if (dropped) return;
            Object.Destroy(_go);
            base.Drop();
        }
    }
}