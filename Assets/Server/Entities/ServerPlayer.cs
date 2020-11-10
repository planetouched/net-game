using System.Collections.Generic;
using Server.Entities._Base;
using Server.Entities.Weapons;
using Shared.Entities;
using Shared.Messages.FromClient;
using UnityEngine;

namespace Server.Entities
{
    public class ServerPlayer : ServerEntityBase
    {
        private readonly SharedPlayer _sharedPlayer;
        public ServerWeapon weapon { get; }

        private GameObject _go;
        private Collider _collider;

        private readonly Queue<ControlMessage> _controlMessages = new Queue<ControlMessage>(256);
        
        private readonly RaycastHit [] _raycastHits = new RaycastHit[10];

        public ServerPlayer(SharedPlayer sharedPlayer) : base(sharedPlayer)
        {
            _sharedPlayer = sharedPlayer;
            _sharedPlayer.isAlive = true;
            _sharedPlayer.weapon = new SharedWeapon();
            
            weapon = new ServerWeapon(_sharedPlayer.weapon);
        }
        
        public override void Create()
        {
            _sharedPlayer.position = new Vector3(5, 1, 0);

            _go = new GameObject("Player: " + _sharedPlayer.objectId);
            _collider = _go.AddComponent<SphereCollider>();
            
            world.AddGameObject(_go);
            _sharedPlayer.LinkTransform(_go.transform);
        }
        
        public void AddControlMessage(ControlMessage message)
        {
            _controlMessages.Enqueue(message);
            _sharedPlayer.lastMessageNum  = message.messageNum;
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
        
        public override void Process(float deltaTime)
        {
            weapon.Prepare();
            
            var rotation = _sharedPlayer.rotation;
            
            _collider.enabled = false;
            
            while (_controlMessages.Count > 0)
            {
                var message = _controlMessages.Dequeue();

                #region movement
                var position = _sharedPlayer.position;
                
                SharedPlayerBehaviour.Movement(ref position, ref rotation, message);

                var moveVector = position - _sharedPlayer.position;
            
                var magnitude = moveVector.magnitude;

                while (magnitude > 0.06f)
                {
                    magnitude -= 0.06f;

                    _sharedPlayer.position = PartialMovement(moveVector.normalized, 0.06f, _sharedPlayer.position);
                }

                _sharedPlayer.position = PartialMovement(moveVector.normalized, magnitude, _sharedPlayer.position); 
                #endregion
               
                _sharedPlayer.rotation = rotation;

                weapon.Use(message.mouseButton0);

                if (weapon.shot && !weapon.isLocked)
                {
                    var target = RewindShot(this, message.serverTime);
                    weapon.Lock();
                    weapon.HitTo(target);
                }
            }
            
            _collider.enabled = true;
            
            weapon.Process(deltaTime);
        }

        public override void Drop()
        {
            if (dropped) return;
            Object.Destroy(_go);
            base.Drop();
        }

        private uint RewindShot(ServerEntityBase shooter, float shooterTime)
        {
            var shooterPlayer = (ServerPlayer) shooter;
            
            if (shooterPlayer.weapon.isInstant)
            {
                var snapshot = world.RewindWorld(shooterTime);
                
                if (snapshot != null)
                {
                    foreach (var checkEntity in snapshot.entities)
                    {
                        //skip self
                        if (checkEntity.Key == shooterPlayer.sharedEntity.objectId) continue;

                        /*
                        var hit = MathUtil.IntersectRaySphere(shooterPlayer.sharedEntity.position, shooterPlayer.sharedEntity.rotation, checkEntity.Value.position, 0.5f);
                        
                        if (hit)
                        {
                            Log.Write("Hit to: " + checkEntity.Key);
                            return checkEntity.Key;
                        }*/
                    }
                }
            }

            return 0;
        }
    }
}