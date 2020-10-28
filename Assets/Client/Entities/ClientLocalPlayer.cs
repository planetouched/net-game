using System.Collections.Generic;
using Client.Entities._Base;
using Client.Entities.Weapons;
using Client.Entities.Weapons._Base;
using Client.Utils;
using Shared.Entities;
using Shared.Entities._Base;
using Shared.Messages.FromClient;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Client.Entities
{
    public class ClientLocalPlayer : ClientEntityBase
    {
        public static uint localObjectId { get; set; }

        private Vector3 _position;
        private Vector3 _rotation;
        private Transform _cameraTransform;
        
        public ClientLocalWeaponBase weapon { get; }
        
        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);
        private ControlMessage _currentControlMessage;

        public ClientLocalPlayer()
        {
            weapon = new ClientLocalRailGun();
        }
        
        public override void SetCurrentEntity(ISharedEntity entity)
        {
            var sharedPlayer = (SharedPlayer) entity;
            weapon.SetCurrentEntity(sharedPlayer.weapon);
            base.SetCurrentEntity(sharedPlayer);
        }        
        
        public void SetCamera(Camera camera)
        {
            _cameraTransform = camera.transform;
        }

        public void AddControlMessage(ControlMessage message, bool localUpdate)
        {
            if (localUpdate)
            {
                _currentControlMessage = message;
            }
            else
            {
                _controlMessages.Enqueue(message);
            }
        }

        public void SetPosition(Vector3 position, Vector3 rotation)
        {
            _position = position;
            _rotation = rotation;
        }
        
        public override void Create()
        {
            _position = current.position;
            _rotation = current.rotation;
        }

        private void UpdateCamera()
        {
            _cameraTransform.rotation = Quaternion.Euler(_rotation.ToUnity());
            _cameraTransform.position = _position.ToUnity();
        }
        
        public override void Process()
        {
            if (_currentControlMessage != null)
            {
                SharedPlayerBehaviour.Movement(ref _position, ref _rotation, _currentControlMessage);

                weapon.Use(_currentControlMessage.mouseButton0);
                weapon.SetPositionAndRotation(_position, _rotation);
                weapon.Process();

                UpdateCamera();
                
                _currentControlMessage = null;
            }
            else
            {
                //rewind
                bool update = _controlMessages.Count > 0;
            
                while (_controlMessages.Count > 0)
                {
                    var message = _controlMessages.Dequeue();
                    SharedPlayerBehaviour.Movement(ref _position, ref _rotation, message);
                }

                if (update)
                {
                    UpdateCamera();
                }
            }
        }
    }
}