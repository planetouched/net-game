using System.Collections.Generic;
using Client.Entities._Base;
using Client.Utils;
using Shared.Entities;
using Shared.Messages.FromClient;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Client.Entities
{
    public class ClientLocalPlayer : ClientEntityBase
    {
        public static uint localObjectId { get; set; }

        private Vector3 _position;
        private Vector3 _rotation;
        private Transform _cameraTransform;
        
        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(64);

        public void SetCamera(Camera camera)
        {
            _cameraTransform = camera.transform;
        }
        
        public void AddControlMessage(ControlMessage message)
        {
            _controlMessages.Enqueue(message);
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

        public override void Process()
        {
            bool update = false;
            
            while (_controlMessages.Count > 0)
            {
                var message = _controlMessages.Dequeue();
                SharedPlayerBehaviour.Movement(ref _position, ref _rotation, message);
                update = true;
            }

            if (update)
            {
                _cameraTransform.rotation = Quaternion.Euler(_rotation.ToUnity());
                _cameraTransform.position = _position.ToUnity();
            }
        }
    }
}