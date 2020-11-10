using LiteNetLib.Utils;
using Shared.Enums;
using UnityEngine;

namespace Shared.Entities._Base
{
    public abstract class SharedEntityBase
    {
        public GameEntityType type { get; protected set; }
        public uint objectId { get; set; }

        public Vector3 position
        {
            get => _linkedTransform != null ? _linkedTransform.position : _position;
            set
            {
                if (_linkedTransform != null)
                {
                    _linkedTransform.position = value;
                }
                else
                {
                    _position = value;
                }
            }
        }

        public Vector3 rotation 
        {
            get => _linkedTransform != null ? _linkedTransform.rotation.eulerAngles : _rotation;
            set
            {
                if (_linkedTransform != null)
                {
                    _linkedTransform.rotation = Quaternion.Euler(value);
                }
                else
                {
                    _rotation = value;
                }
            }
        }

        private Vector3 _position;
        private Vector3 _rotation;

        private Transform _linkedTransform;

        public abstract SharedEntityBase Clone();

        public void LinkTransform(Transform transform)
        {
            _linkedTransform = transform;
            _linkedTransform.position = _position;
            _linkedTransform.rotation = Quaternion.Euler(_rotation);
        }

        protected void WriteHeader(NetDataWriter netDataWriter)
        {
            netDataWriter.Put((byte) type);
            netDataWriter.Put(objectId);

            var pos = position;
                
            netDataWriter.Put(pos.x);
            netDataWriter.Put(pos.y);
            netDataWriter.Put(pos.z);

            var rot = rotation;
            
            netDataWriter.Put(rot.x);
            netDataWriter.Put(rot.y);
            netDataWriter.Put(rot.z);
        }

        public abstract NetDataWriter Serialize(NetDataWriter netDataWriter);

        public abstract void Deserialize(NetDataReader netDataReader);

        protected void ReadHeader(NetDataReader netDataReader)
        {
            type = (GameEntityType) netDataReader.GetByte();
            objectId = netDataReader.GetUInt();

            var pX = netDataReader.GetFloat();
            var pY = netDataReader.GetFloat();
            var pZ = netDataReader.GetFloat();

            var rX = netDataReader.GetFloat();
            var rY = netDataReader.GetFloat();
            var rZ = netDataReader.GetFloat();

            position = new Vector3(pX, pY, pZ);
            rotation = new Vector3(rX, rY, rZ);
        }
    }
}