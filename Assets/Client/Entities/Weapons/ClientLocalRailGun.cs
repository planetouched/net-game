using Basement.OEPFramework.UnityEngine;
using Client.Entities.Weapons._Base;
using Client.Utils;
using Client.Worlds;
using Shared.Entities;
using UnityEngine;

namespace Client.Entities.Weapons
{
    public class ClientLocalRailGun : ClientLocalWeaponBase
    {
        private Timer _timer;
        
        public ClientLocalRailGun(ClientWorld clientWorld) : base(clientWorld)
        {
        }
        
        public override void Create()
        {
            //railgun fps model 
        }

        public override void Process()
        {
            base.Process();
            
            var weapon = (SharedWeapon) current;

            if (weapon.hitTo > 0)
            {
                _timer?.Drop();
                GameObject.Find("Canvas").transform.Find("_Hit").gameObject.SetActive(true);
                _timer = Timer.Create(1, () =>
                {
                    GameObject.Find("Canvas").transform.Find("_Hit").gameObject.SetActive(false);

                }, this, true);
            }
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