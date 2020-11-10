using Client.Entities._Base;
using Client.Entities.Weapons;
using Client.Entities.Weapons._Base;
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
        
        public override void SetCurrentEntity(SharedEntityBase entity)
        {
            _progress = 0;
            var sharedPlayer = (SharedPlayer) entity;
            
            weapon.SetCurrentEntity(sharedPlayer.weapon);
            base.SetCurrentEntity(sharedPlayer);
        }

        public override void Create()
        {
            _go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _go.transform.rotation = Quaternion.Euler(current.rotation);
            _go.transform.position = current.position;
            
            /*
            _go.gameObject.AddComponent<SphereCollider>();
            var rigidbody = _go.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.useGravity = false;*/
        }

        public override void Process()
        {
            if (previous != null)
            {
                _progress += Time.smoothDeltaTime;
                var k = _progress / serverDeltaTime;
                var prevPosition = previous.position;
                var currentPosition = current.position;
                var lerpPosition = Vector3.Lerp(prevPosition, currentPosition, k);
                //_go.transform.position = lerpPosition; 
                
                //it looks better, but what is about accuracy?
                var speed = Vector3.Distance(prevPosition, currentPosition) / serverDeltaTime;
                _go.transform.position = Vector3.MoveTowards(_go.transform.position, lerpPosition, speed * Time.smoothDeltaTime); 
                
                _go.transform.rotation = Quaternion.Lerp(Quaternion.Euler(previous.rotation), Quaternion.Euler(current.rotation), k);
            }
            else
            {
                _go.transform.rotation = Quaternion.Euler(current.rotation);
                _go.transform.position = current.position;
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