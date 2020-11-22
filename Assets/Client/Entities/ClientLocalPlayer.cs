using System.Collections.Generic;
using Client.Entities._Base;
using Client.Entities.Weapons;
using Client.Entities.Weapons._Base;
using Client.Worlds;
using Shared.Entities;
using Shared.Entities._Base;
using Shared.Messages.FromClient;
using UnityEngine;

namespace Client.Entities
{
    public class ClientLocalPlayer : ClientEntityBase
    {
        public static uint serverObjectId { get; set; }

        private Vector3 _position;
        private Vector3 _rotation;
        private Transform _cameraTransform;
        
        public ClientLocalWeaponBase weapon { get; }
        
        private readonly RaycastHit [] _raycastHits = new RaycastHit[10];
        
        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);
        private ControlMessage _currentControlMessage;

        public ClientLocalPlayer(ClientWorld clientWorld) : base(clientWorld)
        {
            weapon = new ClientLocalRailGun(clientWorld);
        }
        
        public override void SetCurrentEntity(SharedEntityBase entity)
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
            _cameraTransform.rotation = Quaternion.Euler(_rotation);
            _cameraTransform.position = _position;
        }
        
        public override void Process()
        {
            if (_currentControlMessage != null)
            {
                ApplyMovement(_currentControlMessage);

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
                    ApplyMovement(_controlMessages.Dequeue());
                }

                if (update)
                {
                    UpdateCamera();
                }
            }
        }

        private Vector3 PartialMovement(Vector3 moveVectorNormalized, float distance, Vector3 lastGoodPosition)
        {
            var size = Physics.SphereCastNonAlloc(lastGoodPosition, 0.5f, moveVectorNormalized, _raycastHits, distance);
            
            if (size > 0)
            {
                var normal = _raycastHits[0].normal;

                for (int i = 1; i < size; i++)
                {
                    normal += _raycastHits[i].normal;
                }
                
                var newPosition = lastGoodPosition;
                
                var normalProject = Vector3.ProjectOnPlane(normal, Vector3.up);
                var normalPerpendicular = Vector3.Cross(normalProject, Vector3.up);
                var velocityProject = Vector3.Project(moveVectorNormalized * distance, normalPerpendicular);
                newPosition += velocityProject;

                if (!Physics.CheckSphere(newPosition, 0.5f))
                {
                    return newPosition;
                }
                
                //stack
                return lastGoodPosition;
            }
            
            return lastGoodPosition + moveVectorNormalized * distance;
        }
        
        private void ApplyMovement(ControlMessage message)
        {
            var pos = _position;

            SharedPlayerBehaviour.Movement(ref pos, ref _rotation, message);

            #region movement
            var moveVector = pos - _position;
            
            var magnitude = moveVector.magnitude;

            while (magnitude > 0.06f)
            {
                magnitude -= 0.06f;

                _position = PartialMovement(moveVector.normalized, 0.06f, _position);
            }

            _position = PartialMovement(moveVector.normalized, magnitude, _position);
            #endregion
        }
    }
}