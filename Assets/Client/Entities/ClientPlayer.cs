using Client.Entities._Base;
using Client.Utils;
using Shared.Entities._Base;
using UnityEngine;

namespace Client.Entities
{
    public class ClientPlayer : ClientEntityBase
    {
        private GameObject _go;
        private float _progress;

        public override void SetCurrentEntity(ISharedEntity entity)
        {
            _progress = 0;
            base.SetCurrentEntity(entity);
        }

        public override void Create()
        {
            _go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _go.transform.rotation = Quaternion.Euler(current.rotation.ToUnity());
            _go.transform.position = current.position.ToUnity();
        }

        public override void Process()
        {
            if (previous != null)
            {
                _progress += Time.deltaTime;
                var k = _progress / serverDeltaTime;
                _go.transform.position = Vector3.Lerp(previous.position.ToUnity(), current.position.ToUnity(), k);
                _go.transform.rotation = Quaternion.Lerp(Quaternion.Euler(previous.rotation.ToUnity()), Quaternion.Euler(current.rotation.ToUnity()), k);
            }
            else
            {
                _go.transform.rotation = Quaternion.Euler(current.rotation.ToUnity());
                _go.transform.position = current.position.ToUnity();
            }
        }

        public override void Drop()
        {
            if (dropped) return;
            Object.Destroy(_go);
            base.Drop();
        }
    }
}