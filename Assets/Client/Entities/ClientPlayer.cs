using Client.Entities._Base;
using Client.Utils;
using LiteNetLib.Utils;
using Shared.Enums;
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

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            //lastMessageNum = netDataReader.GetUInt();
            netDataReader.SkipBytes(4);
        }

        public override void Process()
        {
            _go.transform.rotation = Quaternion.Euler(rotation.ToUnity());
            _go.transform.position = position.ToUnity();
        }

        public override void Drop()
        {
            if (dropped) return;
            Object.Destroy(_go);
            base.Drop();
        }
    }
}