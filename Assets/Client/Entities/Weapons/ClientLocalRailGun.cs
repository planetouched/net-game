using Client.Entities.Weapons._Base;
using Client.Utils;
using UnityEngine;

namespace Client.Entities.Weapons
{
    public class ClientLocalRailGun : ClientLocalWeaponBase
    {
        public override void Create()
        {
            //railgun fps model 
        }
        
        protected override void Shot()
        {
            var obj = Object.Instantiate(Resources.Load<GameObject>("RailGun/Prefabs/RailGun"));
            obj.transform.position = position.ToUnity();
            obj.transform.rotation = Quaternion.Euler(rotation.ToUnity());
            obj.transform.Translate(Vector3.forward * 0.5f);
        }
    }
}