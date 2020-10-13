using Client.Entities._Base;
using Client.Utils;
using Shared.Enums;
using Shared.Utils;
using UnityEngine;

namespace Client.Entities
{
    public class ClientPlayer : ClientEntityBase
    {
        private GameObject _go;

        public ClientPlayer()
        {
            type = GameEntityType.Player;
        }

        public override void Create()
        {
            _go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _go.transform.rotation = Quaternion.Euler(rotation.ToUnity());
            _go.transform.position = position.ToUnity();
        }

        public override void Process()
        {
            _go.transform.rotation = Quaternion.Euler(rotation.ToUnity());
            _go.transform.position = position.ToUnity();
        }

        public override void Deserialize(ref int offset, byte[] buffer)
        {
            type = (GameEntityType) SerializeUtil.GetByte(ref offset, buffer);
            objectId = SerializeUtil.GetUInt(ref offset, buffer);
            //skip one
            offset += 4;
            //lastMessageNum = SerializeUtil.GetUInt(ref offset, buffer);
            position = SerializeUtil.GetVector3(ref offset, buffer);
            rotation = SerializeUtil.GetVector3(ref offset, buffer);
        }

        public override void Drop()
        {
            if (dropped) return;
            Object.Destroy(_go);
            base.Drop();
        }
    }
}